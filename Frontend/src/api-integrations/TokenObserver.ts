import { handleAccessTokenCheck, handleRefreshTokenUpdate } from "./TokenValidationAPI.ts";
import { StorageGetItem, StorageSetItem, StorageDeleteItem } from '../vk-integrations/cloudstorage/CloudStorage.ts';
import WebApp from "@twa-dev/sdk";

const AccessTokenUpdate = async () => {

    const refreshTokens: string = await StorageGetItem("RefreshToken");

    if (refreshTokens !== "empty") {
        try {
            await handleRefreshTokenUpdate(refreshTokens);
            return true;
        }
        catch (e) {
            alert("Токен обновления недействителен! " + refreshTokens)
        
            console.log("Токен обновления недействителен!");
            return false;
        }
    }

    return false;
}

const TokenNeedUpdate = async () => {
    var upd = await AccessTokenUpdate();

    return upd;
}

export { TokenNeedUpdate }