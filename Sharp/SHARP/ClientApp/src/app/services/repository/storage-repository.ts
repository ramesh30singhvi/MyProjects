import { IRepository } from './types';

export abstract class StorageRepository<T> implements IRepository<T> {
    protected name: string;
    storage: Storage;

    constructor(name: string, storage: Storage) {
        this.name = name;
        this.storage = storage;
    }
    load() {
        const value = this.storage.getItem(this.name);
        return value === undefined || value === 'undefined' ? undefined : JSON.parse(value);
    }
    clear() {
        this.storage.removeItem(this.name);
    }
    save(objectForSave: T) {
        this.storage.setItem(this.name, JSON.stringify(objectForSave));
    }
    add(objectForSave: T) {
        const savedSate = this.load();
        this.save({
            ...savedSate,
            ...objectForSave,
        });
    }
}