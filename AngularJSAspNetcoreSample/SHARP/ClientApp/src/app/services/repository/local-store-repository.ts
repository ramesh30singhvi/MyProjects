import { StorageRepository } from './storage-repository';

export class LocalStoreRepository<T> extends StorageRepository<T> {
    constructor(name: string) {
        super(name, localStorage)
    }
}