import axios from 'axios';
import { StorageGetItem } from '../telegram-integrations/cloudstorage/CloudStorage';
import { TokenNeedUpdate } from './TokenObserver.ts';

var CLIENT_API_URL = import.meta.env.VITE_CLIENT_API;

const handleRestaurantRequest = async (
    restaurantName: string,
    address: string,
    phone_number: string,
    description: string,
    imagePath: string,
    open_time: string,
    close_time: string,
    request_description: string,
    accessToken: string,
    retry: boolean = true
) => {
    try {
        const response = await axios.post(
            `${CLIENT_API_URL}/api/RequestRoles/Restaurant`,
            {
                restaurantName,
                address,
                phone_number,
                description,
                imagePath,
                open_time,
                close_time,
                request_description
            },
            {
                headers: {
                    Authorization: `Bearer ${accessToken}`
                }
            }
        );

        return response.status === 200;
    } catch (error) {
        if (axios.isAxiosError(error)) {
            if (error.response) {
                if (error.response.status === 401 && retry) { 
                    console.log("Повторный запрос!"); 

                    if (await TokenNeedUpdate()) {
                        const newAccessToken: string = await StorageGetItem("AccessToken");

                        if (newAccessToken !== "empty")
                            return handleRestaurantRequest(
                                restaurantName,
                                address,
                                phone_number,
                                description,
                                imagePath,
                                open_time,
                                close_time,
                                request_description,
                                newAccessToken,
                                false
                            );
                    }
                } else {
                    console.log(`Ошибка: ${error.response.status}`);
                }
            } else {
                console.log("Неизвестная ошибка");
            }
        }
        return false;
    }
};

const handleCourierRequest = async (carNumber: string, description: string, accessToken: string, retry: boolean = true) => {
    try {
        const response = await axios.post(
            `${CLIENT_API_URL}/api/RequestRoles/Courier`,
            { 
                car_number: carNumber, 
                request_description: description 
            },
            {
                headers: {
                    Authorization: `Bearer ${accessToken}`
                }
            }
        );

        return response.status === 200;
    } catch (error) {
        if (axios.isAxiosError(error)) {
            if (error.response) {
                if (error.response.status === 401 && retry) { 
                    console.log("Повторный запрос!"); 

                    if (await TokenNeedUpdate()) {
                        const newAccessToken: string = await StorageGetItem("AccessToken");

                        if (newAccessToken !== "empty")
                            return handleCourierRequest(carNumber, description, newAccessToken, false);
                    }
                } else {
                    console.log(`Ошибка: ${error.response.status}`);
                }
            } else {
                console.log("Неизвестная ошибка");
            }
        }
        return false;
    }
};


export { handleRestaurantRequest, handleCourierRequest }