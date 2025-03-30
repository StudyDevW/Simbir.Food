import axios from 'axios';
import { StorageGetItem } from '../telegram-integrations/cloudstorage/CloudStorage.ts';
import { TokenNeedUpdate } from './TokenObserver.ts';
import { FoodItemInfo, RestaurantInfo } from './Interfaces/API_Interfaces.ts';

var RESTAURANT_API_URL = import.meta.env.VITE_RESTAURANT_API;

const handleRestaurantsInfo = async (accessToken: string, retry: boolean = true) : Promise<any> => {

    try {
        const response = await axios.get(`${RESTAURANT_API_URL}/api/Restaurant/GetAllRestaurant`,
        {
            headers: {
                Authorization: `Bearer ${accessToken}`
            },
        });

        if (response.status === 200) {

            const result: RestaurantInfo[] = response.data;
           
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
                            return handleRestaurantsInfo(accessTokens, false);
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

const handleFoodItemsInfo = async (accessToken: string, restaurantId: string, retry: boolean = true) : Promise<any> => {
    try {
        const response = await axios.get(`${RESTAURANT_API_URL}/api/RestaurantFoodItems/GetRestaurantFoodItems/${restaurantId}`,
        {
            headers: {
                Authorization: `Bearer ${accessToken}`
            },
        });

        if (response.status === 200) {

            const result: FoodItemInfo[] = response.data;
           
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
                            return handleFoodItemsInfo(accessTokens, restaurantId, false);
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

export { handleRestaurantsInfo, handleFoodItemsInfo }