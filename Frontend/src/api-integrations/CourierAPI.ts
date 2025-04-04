import { OrderForCourierDto } from './Interfaces/API_Interfaces.ts';
import { TokenNeedUpdate } from './TokenObserver.ts';
import { StorageGetItem } from '../telegram-integrations/cloudstorage/CloudStorage';
import axios from 'axios';


var COURIER_API_URL = import.meta.env.VITE_COURIER_API;

const handleOrdersForCourier = async (accessToken: string, retry: boolean = true) : Promise<OrderForCourierDto[] | null> => {

    try {
        const response = await axios.get(`${COURIER_API_URL}/api/courier/ordersForCourier`,
        {
            headers: {
                Authorization: `Bearer ${accessToken}`
            },
        });

        if (response.status === 200) {

            const result: OrderForCourierDto[] = response.data;
           
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
                            return handleOrdersForCourier(accessTokens, false);
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

const handleOrderAccept = async (accessToken: string, orderId: string, retry: boolean = true) : Promise<any> => {

    try {
        const response = await axios.post(`${COURIER_API_URL}/api/courier/${orderId}/accept`,
        {},
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
                            return handleOrderAccept(accessTokens, orderId, false);
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

const handleOrderCourierInPlace = async (accessToken: string, orderId: string, retry: boolean = true) : Promise<any> => {

    try {
        const response = await axios.post(`${COURIER_API_URL}/api/courier/${orderId}/courierOnPlace`,
        {},
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
                            return handleOrderCourierInPlace(accessTokens, orderId, false);
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

const handleOrderDelivered = async (accessToken: string, orderId: string, retry: boolean = true) : Promise<any> => {

    try {
        const response = await axios.post(`${COURIER_API_URL}/api/courier/${orderId}/delivered`,
        {},
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
                            return handleOrderDelivered(accessTokens, orderId, false);
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


export { handleOrdersForCourier, handleOrderCourierInPlace, handleOrderDelivered, handleOrderAccept }