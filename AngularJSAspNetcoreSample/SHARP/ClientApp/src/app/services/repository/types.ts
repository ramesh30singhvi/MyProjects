export interface IRepository<T> {
    load(): T;
    clear(): void;
    save(objectForSave: T): void;
    add(objectForSave: T): void;
}