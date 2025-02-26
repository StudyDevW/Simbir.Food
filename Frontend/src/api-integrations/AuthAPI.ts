import axios from 'axios';
import { StorageGetItem, StorageSetItem, StorageDeleteItem } from '../telegram-integrations/cloudstorage/CloudStorage.ts';
import { TokenNeedUpdate } from './TokenObserver.ts';

var CLIENT_API_URL = import.meta.env.VITE_CLIENT_API;

interface AuthComponent {
    id: number,
    first_name: string,
    last_name: string | null,
    username: string | null,
    is_bot: boolean,
    photo_url: string | null,
    chat_id: number,
    address: string,
    device: string,
    roles: string[]
}

const handleUserAuth = async (authvars: AuthComponent):  Promise<any> => {
    try {
        const response = await axios.post(`${CLIENT_API_URL}/api/Auth/UserAuth`, authvars);

        if (response.status === 200) {

            const { accessToken, refreshToken } = response.data;

            StorageSetItem("AccessToken", accessToken);
            StorageSetItem("RefreshToken", refreshToken);

            return true;
        }

        return false;
    }
    catch (error) {
        return false;
    }
}

const handleUserRegister = async (authvars: AuthComponent):  Promise<any> => {
    try {
        const response = await axios.post(`${CLIENT_API_URL}/api/Auth/UserRegister`, authvars);

        if (response.status === 200) 
            return response.data;

        return null;
    }
    catch (error) {
        return null;
    }
}


export { handleUserAuth, handleUserRegister } 