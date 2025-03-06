import axios from 'axios';
import { StorageGetItem, StorageSetItem, StorageDeleteItem } from '../telegram-integrations/cloudstorage/CloudStorage.ts';
import { TokenNeedUpdate } from './TokenObserver.ts';
import { AuthComponent } from './Interfaces/API_Interfaces.ts';

var CLIENT_API_URL = import.meta.env.VITE_CLIENT_API;


const handleUserAuth = async (authvars: AuthComponent):  Promise<any> => {
    try {
        const response = await axios.patch(`${CLIENT_API_URL}/api/Auth/UserAuth`, authvars);

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