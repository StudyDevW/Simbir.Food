import axios from 'axios';
import { StorageGetItem, StorageSetItem, StorageDeleteItem } from '../telegram-integrations/cloudstorage/CloudStorage.ts';
import { TokenNeedUpdate } from './TokenObserver.ts';

var CLIENT_API_URL = import.meta.env.VITE_CLIENT_API;

interface GetMeInfo {
    Id: string,
    telegram_id: number,
    first_name: string,
    last_name: string | null,
    username: string | null,
    photo_url: string | null,
    chat_id: number,
    address: string | null,
    restaurant_own: string[] | null,
    roles: string[]
}

const handleGetInfoMe = async (accessToken: string, retry: boolean = true)  : Promise<any> => {

    try {
        const response = await axios.get(`${CLIENT_API_URL}/api/Clients/Me`,
        {
            headers: {
                Authorization: `Bearer ${accessToken}`
            },
        });

        if (response.status === 200) {
            return <GetMeInfo>response.data;
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
                            return handleGetInfoMe(accessTokens, false);
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

export { handleGetInfoMe }