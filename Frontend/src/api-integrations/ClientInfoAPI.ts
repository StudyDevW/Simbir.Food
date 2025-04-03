import axios from 'axios';
import { StorageGetItem, StorageSetItem, StorageDeleteItem } from '../telegram-integrations/cloudstorage/CloudStorage.ts';
import { TokenNeedUpdate } from './TokenObserver.ts';
import { GetMeInfo, ClientGetAll } from './Interfaces/API_Interfaces.ts';
import { RestaurantsInfoForOwner } from './Interfaces/API_Interfaces.ts';

var CLIENT_API_URL = import.meta.env.VITE_CLIENT_API;


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

const handleGetInfoForAllUsers = async (
    from: number = 0,
    count: number = 10
): Promise<ClientGetAll | null> => {
    const accessToken = await StorageGetItem("AccessToken");

    try {
        const response = await axios.get<ClientGetAll>(
            `${CLIENT_API_URL}/api/Clients`,
            {
                headers: { 
                    Authorization: `Bearer ${accessToken}`,
                    'Content-Type': 'application/json'
                },
                params: { from, count }
            }
        );
        
        return response.data;
    } catch (error) {
        console.error('API request failed:', error);
        if (axios.isAxiosError(error) && error.response?.status === 401) {
            if (await TokenNeedUpdate()) {
                const newAccessToken = await StorageGetItem("AccessToken");
                if (newAccessToken !== "empty") {
                    return handleGetInfoForAllUsers(from, count);
                }
            }
        }
        return null;
    }
};

const handleRestaurantsForUser = async (accessToken: string, retry: boolean = true) : Promise<RestaurantsInfoForOwner[] | null> => {

    try {
        const response = await axios.get(`${CLIENT_API_URL}/api/Clients/Restaurants`,
        {
            headers: {
                Authorization: `Bearer ${accessToken}`
            },
        });

        if (response.status === 200) {

            const result: RestaurantsResponse = response.data;
           
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
                            return handleRestaurantsForUser(accessTokens, false);
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

export { handleGetInfoMe, handleGetInfoForAllUsers, handleRestaurantsForUser }