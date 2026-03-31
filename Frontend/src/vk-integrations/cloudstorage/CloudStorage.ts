//import WebApp from '@twa-dev/sdk';
 
import vkBridge from '@vkontakte/vk-bridge';

// const StorageGetItemAsync = (key: string): Promise<string | undefined> => {
//     return new Promise((resolve, reject) => {
//       WebApp.CloudStorage.getItem(key, (error, result) => {
//         if (error) {
//             reject(error);
//         } else {
//             resolve(result);
//         }
//       });
//     });
// }

const StorageGetItem = async (key: string): Promise<string> => {
    try {
        const result = await vkBridge.send('VKWebAppStorageGet', {
            keys: [key]
        });
        
        // result имеет структуру: { keys: [{ key: "AccessToken", value: "..." }] }
        if (result.keys && result.keys.length > 0) {
            return result.keys[0].value || "empty";
        }
        
        return "empty";
    } catch (error) {
        console.error('Ошибка получения данных из Storage:', error);
        return "empty";
    }
}

const StorageSetItem = (key: string, value: string) => {
    // WebApp.CloudStorage.setItem(key, value);

    //Порт под ВК (study)
    vkBridge.send('VKWebAppStorageSet', { 
        key: key,
        value: value
    })
}

const StorageDeleteItem = (key: string) => {
    //WebApp.CloudStorage.removeItem(key);

       //Порт под ВК (study)
    vkBridge.send('VKWebAppStorageSet', { 
        key: key,
        value: ''
    })
}

export { StorageGetItem, StorageSetItem, StorageDeleteItem }