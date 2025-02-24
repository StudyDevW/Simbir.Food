import axios from 'axios';
import { StorageGetItemAsync, StorageSetItem, StorageDeleteItem } from '../cloudstorage-telegram/CloudStorage.ts';
import { TokenNeedUpdate } from './TokenObserver.ts';


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

var CLIENT_API_URL = import.meta.env.VITE_CLIENT_API;


const handleUserAuth = async (authvars: AuthComponent):  Promise<any> => {
    try {
        const response = await axios.post(`${CLIENT_API_URL}/api/Auth/UserAuth`, authvars);

        if (response.status === 200) {

            if (response.data === "register_request_created" || response.data === "register_request_already_exist") {

                StorageSetItem("RegisterCallback", "register_requested");

                return "register";
            }
            else {
                const { accessToken, refreshToken } = response.data;

                StorageSetItem("RegisterCallback", "logined");

                StorageSetItem("AccessToken", accessToken);
                StorageSetItem("RefreshToken", refreshToken);

                return "logined";
            }
        }

        return null;
    }
    catch (error) {
        alert(error)
        return null;
    }
}

export { handleUserAuth } 