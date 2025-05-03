'use server';
import { auth } from '@/auth';
import { fetchWrapper } from '@/lib/fetchWrapper';
import { Auction, PagedResult } from '@/types';
import { stat } from 'fs';
import { revalidatePath } from 'next/cache';
import { FieldValue, FieldValues } from 'react-hook-form';

// fetch the data from the api server
export async function getData(query: string): Promise<PagedResult<Auction>> {
    return await fetchWrapper.get(`search${query}`);
}

export async function updateAuctionTest() {
    console.log('update test calisti')
    const data = {
        mileage: Math.floor(Math.random() * 10000) + 1
    }
    console.log('fetch weapper da calisacak test calisti')

    return await fetchWrapper.put('auctions/466e4744-4dc5-4987-aae0-b621acfc5e39', data);
}

export async function createAuction(data: FieldValues) {
    return await fetchWrapper.post('auctions', data);
}
export async function getDetailedViewData(id: string): Promise<Auction> {
    return await fetchWrapper.get(`auctions/${id}`);
}

export async function updateAuction(data: FieldValues, id: string) {
    const res = await fetchWrapper.put(`auctions/${id}`, data);
    revalidatePath(`/auctions/${id}`);
    return res;
}

export async function deleteAuction(id: string) {
    return await fetchWrapper.del(`auctions/${id}`);
}