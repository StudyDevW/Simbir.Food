import axios from 'axios';
import { StorageGetItem } from '../telegram-integrations/cloudstorage/CloudStorage.ts';
import { TokenNeedUpdate } from './TokenObserver.ts';

var RESTAURANT_API_URL = import.meta.env.VITE_RESTAURANT_API;

const handleLoadImage = async (filePath: string) => {
    try {
        const response = await axios.get(`${RESTAURANT_API_URL}/api/Photos/Image`, {
            responseType: 'blob',
            headers: {
                filePath: filePath
            },
        });

        return URL.createObjectURL(response.data);

    } catch (error) {
        console.log("Внутренняя ошибка получения информации о товаре!")
        return null
    } 
}

export { handleLoadImage }