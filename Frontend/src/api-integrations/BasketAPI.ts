import axios from 'axios';
import { StorageGetItem } from '../telegram-integrations/cloudstorage/CloudStorage.ts';
import { GetBasketInfo } from './Interfaces/API_Interfaces.ts';
import { TokenNeedUpdate } from './TokenObserver.ts';

var CLIENT_API_URL = import.meta.env.VITE_CLIENT_API;

const handleGetBasketInfo = async (accessToken: string, retry: boolean = true): Promise<any> => {
    try {
        const response = await axios.get(`${CLIENT_API_URL}/api/Basket`,
        {
            headers: {
                Authorization: `Bearer ${accessToken}`
            },
        });

        if (response.status === 200) {
            return <GetBasketInfo>response.data;
        }

        return null;
    }
    catch (error) {
        if (axios.isAxiosError(error)) {
            if (error.response) {
                if (error.response.status === 401 && retry) {

                    console.log("Повторный запрос!");

                    if (await TokenNeedUpdate()) {
                        const accessTokens: string = await StorageGetItem("AccessToken");

                        if (accessTokens !== "empty")
                            return handleGetBasketInfo(accessTokens, false);
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

        return null;
    }
}

const handleBasketDeleteItem = async (basketItemId: string, accessToken: string, retry: boolean = true): Promise<any> => {
    try {
        const response = await axios.delete(`${CLIENT_API_URL}/api/Basket/${basketItemId}`,
        {
            headers: {
                Authorization: `Bearer ${accessToken}`
            },
        });

        if (response.status === 200) {
            return true;
        }

        return false;
    }
    catch (error) {
        if (axios.isAxiosError(error)) {
            if (error.response) {
                if (error.response.status === 401 && retry) {

                    console.log("Повторный запрос!");

                    if (await TokenNeedUpdate()) {
                        const accessTokens: string = await StorageGetItem("AccessToken");

                        if (accessTokens !== "empty")
                            return handleBasketDeleteItem(basketItemId, accessTokens, false);
                    }
                }
                else {
                    console.log(`Ошибка: ${error.response.status}`);
                }
            }
            else {
                console.log("Неизвестная ошибка");
                return false;
            }
        }

        return false;
    }
}


export { handleGetBasketInfo, handleBasketDeleteItem }