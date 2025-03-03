import WebApp from '@twa-dev/sdk';

const StorageGetItemAsync = (key: string): Promise<string | undefined> => {
    return new Promise((resolve, reject) => {
      WebApp.CloudStorage.getItem(key, (error, result) => {
        if (error) {
            reject(error);
        } else {
            resolve(result);
        }
      });
    });
}

const StorageGetItem = async (key: string) => {
    var getitem = await StorageGetItemAsync(key);

    if (getitem !== undefined)
        return getitem;

    return "empty";
}

const StorageSetItem = (key: string, value: string) => {
    WebApp.CloudStorage.setItem(key, value);
}

const StorageDeleteItem = (key: string) => {
    WebApp.CloudStorage.removeItem(key);
}

export { StorageGetItem, StorageSetItem, StorageDeleteItem }