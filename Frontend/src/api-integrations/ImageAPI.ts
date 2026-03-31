import axios from 'axios';
import { StorageGetItem } from '../vk-integrations/cloudstorage/CloudStorage.ts';
import { TokenNeedUpdate } from './TokenObserver.ts';

var RESTAURANT_API_URL = import.meta.env.VITE_RESTAURANT_API;

const handleLoadImage = async (accessToken: string, filePath: string, retry: boolean = true): Promise<any> => {
    try {
        const response = await axios.get(`${RESTAURANT_API_URL}/api/Photos/Image`, {
            responseType: 'blob',
            headers: {
                Authorization: `Bearer ${accessToken}`,
                filePath: filePath
            },
        });

        return URL.createObjectURL(response.data);

    } catch (error) {
        if (axios.isAxiosError(error)) {
            if (error.response) {
                if (error.response.status === 401 && retry) {

                    console.log("Повторный запрос!");

                    if (await TokenNeedUpdate()) {

                        const accessTokens: string = await StorageGetItem("AccessToken");

                        if (accessTokens !== "empty")
                            return handleLoadImage(accessTokens, filePath, false);
                    }
                }
                else {
                    console.log(`Ошибка: ${error.response.status}`);
                }
            }
            else {
                console.log("Неизвестная ошибка");
                return null;
            }
        }

        return null
    } 
}

export { handleLoadImage }