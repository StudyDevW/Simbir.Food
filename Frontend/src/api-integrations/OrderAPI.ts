import axios from 'axios';
import { StorageGetItem } from '../telegram-integrations/cloudstorage/CloudStorage.ts';
import { TokenNeedUpdate } from './TokenObserver.ts';
import { OrderInfo } from './Interfaces/API_Interfaces.ts';

var CLIENT_API_URL = import.meta.env.VITE_CLIENT_API;

const handleOrderCreate = async (accessToken: string, retry: boolean = true): Promise<any> => {
    try {
        const response = await axios.post(`${CLIENT_API_URL}/api/Order/Create`, {},
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
                            return handleOrderCreate(accessTokens, false);
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

const handleOrdersGet = async (accessToken: string, retry: boolean = true): Promise<any> => {
    try {
        const response = await axios.get(`${CLIENT_API_URL}/api/Order/Info`,
        {
            headers: {
                Authorization: `Bearer ${accessToken}`
            },
        });

        if (response.status === 200) {

            const result: OrderInfo[] = response.data;
           
            return result;
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
                            return handleOrdersGet(accessTokens, false);
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

export { handleOrderCreate, handleOrdersGet } 