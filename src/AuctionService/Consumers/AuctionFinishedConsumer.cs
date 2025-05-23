using System;
using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    private readonly AuctionDbContext _context;

    public AuctionFinishedConsumer(AuctionDbContext dbContext)
    {
        _context = dbContext;
    }

    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        var auction = await _context.Auctions.FindAsync(context.Message.AuctionId);

        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = context.Message.Amount;
        }

        auction.Status = auction.SoldAmount > auction.ReservePrice
        ? Status.Finished : Status.ReserveNotMet;

        await _context.SaveChangesAsync();
    }
}
