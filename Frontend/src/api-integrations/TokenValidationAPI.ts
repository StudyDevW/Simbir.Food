import axios from 'axios';
import { StorageGetItem, StorageSetItem, StorageDeleteItem } from '../telegram-integrations/cloudstorage/CloudStorage.ts';
import { TokenNeedUpdate } from './TokenObserver.ts';

var CLIENT_API_URL = import.meta.env.VITE_CLIENT_API;

const handleAccessTokenCheck = async (accessToken: string, retry: boolean = true): Promise<any> => {

    try {
        const response = await axios.get(`${CLIENT_API_URL}/api/Auth/Validate`, {
            headers: {
                accessToken: accessToken,
            },
        });


        if (response.status === 200) {
            console.log('Токен валид!');
            return true
        }

        console.log('Токен не действительный!');
        return false
    }
    catch (error) {
        if (axios.isAxiosError(error)) {

            if (error.response) {
                if (error.response.status === 401 && retry) {

                    console.log("Повторный запрос!");

                    if (await TokenNeedUpdate()) {

                        const accessTokens: string = await StorageGetItem("AccessToken");

                        if (accessTokens !== "empty")
                            return handleAccessTokenCheck(accessTokens, false);
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
    }
}

const handleRefreshTokenUpdate = async (refreshTokenIn: string) => {

    const response = await axios.post(`${CLIENT_API_URL}/api/Auth/Refresh`, {
        refreshToken: refreshTokenIn
    });

    if (response.status === 200) {
        console.log('Токены обновлены!:', response.data);

        const { accessToken, refreshToken } = response.data;

        StorageSetItem("AccessToken", accessToken);
        StorageSetItem("RefreshToken", refreshToken);

        

        return true
    }

    console.log('Ошибка обновления токенов!');
    return false;

}

const handleLoginSignOut = async (accessToken: string, retry: boolean = true) : Promise<any> => {
    try {
        const response = await axios.put(`${CLIENT_API_URL}/api/Authentication/SignOut`, {},
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
                            return handleLoginSignOut(accessTokens, false);
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
const LoginSignOut = async () => {

    const accessTokens: string = await StorageGetItem("AccessToken");
    if (accessTokens !== "empty") {
        if (await handleLoginSignOut(accessTokens)) {
            StorageDeleteItem("AccessToken");
            StorageDeleteItem("RefreshToken");
        }
    }

    console.log('Выход из аккаунта!');
}

export { 
    handleAccessTokenCheck, 
    handleRefreshTokenUpdate, 
    LoginSignOut, 
    handleLoginSignOut
}