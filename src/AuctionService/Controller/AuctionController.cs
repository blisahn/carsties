using System;
using AuctionService.Data;
using AuctionService.DTO;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controller;

[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEnpoint;
    private readonly AuctionDbContext _context;

    public AuctionController(AuctionDbContext context, IMapper mapper,
     IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEnpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();
        if (!string.IsNullOrWhiteSpace(date))
        {
            query = query.Where(x =>
                x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0
            );
        }
        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();

        // var auctions = await _context.Auctions --> Previos version
        //     .Include(x => x.Item)
        //     .OrderBy(x => x.Item.Make)
        //     .ToListAsync();

        // return _mapper.Map<List<AuctionDto>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context
            .Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null)
        {
            return NotFound();
        }
        return _mapper.Map<AuctionDto>(auction);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        // TODO: add current user as seller
        auction.Seller = User.Identity.Name;
        _context.Auctions.Add(auction);
        var newAuction = _mapper.Map<AuctionDto>(auction);

        await _publishEnpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
            return BadRequest("Could not save changes to the DB");

        return CreatedAtAction(
            nameof(GetAuctionById),
            new { auction.Id }, newAuction);
    }


    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context
            .Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
            return NotFound();

        //TODO: check seller == username
        if (auction.Seller != User.Identity.Name)
        {
            return Forbid();
        }

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        await _publishEnpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

        var result = await _context.SaveChangesAsync() > 0;

        if (result)
            return Ok();
        return BadRequest("Problem saving changes");
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if (auction == null)
            return NotFound();

        //Todo: Check seller == username
        if (auction.Seller != User.Identity.Name)
        {
            return Forbid();
        }
        _context.Auctions.Remove(auction);
        await _publishEnpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });
        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
            return BadRequest("Could not update DB");
        return Ok();
    }
}
