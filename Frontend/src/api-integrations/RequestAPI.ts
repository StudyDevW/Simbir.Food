import axios from 'axios';
import { StorageGetItem } from '../vk-integrations/cloudstorage/CloudStorage.ts';
import { TokenNeedUpdate } from './TokenObserver.ts';
import { RequestsGetAll } from './Interfaces/API_Interfaces.ts';

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
): Promise<any> => {
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

const handleCourierRequest = async (carNumber: string, description: string, accessToken: string, retry: boolean = true): Promise<any> => {
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

const handleGetRequests = async (accessToken: string, retry: boolean = true): Promise<RequestsGetAll | null> => {
    try {
        const response = await axios.get<RequestsGetAll>(
            `${CLIENT_API_URL}/api/RequestRoles`,
            {
                headers: {
                    Authorization: `Bearer ${accessToken}`
                }
            }
        );

        return response.data;
    } catch (error) {
        if (axios.isAxiosError(error)) {
            if (error.response) {
                if (error.response.status === 401 && retry) { 
                    console.log("Retrying request with new token..."); 

                    if (await TokenNeedUpdate()) {
                        const newAccessToken: string = await StorageGetItem("AccessToken");

                        if (newAccessToken !== "empty") {
                            return handleGetRequests(newAccessToken, false);
                        }
                    }
                } else {
                    console.error(`Error: ${error.response.status}`, error.response.data);
                }
            } else {
                console.error("Network error:", error.message);
            }
        } else {
            console.error("Unknown error:", error);
        }
        return null;
    }
};

const handleApproveRestaurantRequest = async (request_id: string, accessToken: string, retry: boolean = true): Promise<any> => {
    try {
        const response = await axios.put(
            `${CLIENT_API_URL}/api/RequestRoles/Restaurant/Accept`,
            { 
                requestId: request_id, 
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
                            return handleApproveRestaurantRequest(request_id, newAccessToken, false);
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

const handleRejectRestaurantRequest = async (request_id: string, accessToken: string, retry: boolean = true): Promise<any> => {
    try {
        const response = await axios.put(
            `${CLIENT_API_URL}/api/RequestRoles/Restaurant/Reject`,
            { 
                requestId: request_id, 
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
                            return handleRejectRestaurantRequest(request_id, newAccessToken, false);
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

const handleApproveCourierRequest = async (request_id: string, accessToken: string, retry: boolean = true): Promise<any> => {
    try {
        const response = await axios.put(
            `${CLIENT_API_URL}/api/RequestRoles/Courier/Accept`,
            { 
                requestId: request_id, 
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
                            return handleApproveCourierRequest(request_id, newAccessToken, false);
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

const handleRejectCourierRequest = async (request_id: string, accessToken: string, retry: boolean = true): Promise<any> => {
    try {
        const response = await axios.put(
            `${CLIENT_API_URL}/api/RequestRoles/Courier/Reject`,
            { 
                requestId: request_id, 
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
                            return handleRejectCourierRequest(request_id, newAccessToken, false);
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

export { handleRestaurantRequest, handleCourierRequest, handleGetRequests, handleApproveRestaurantRequest, handleRejectRestaurantRequest, handleApproveCourierRequest, handleRejectCourierRequest }