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

export { handleOrdersForCourier }