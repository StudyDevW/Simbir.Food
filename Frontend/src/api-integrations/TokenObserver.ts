import { handleAccessTokenCheck, handleRefreshTokenUpdate } from "./TokenValidationAPI.ts";
import { StorageGetItemAsync, StorageSetItem, StorageDeleteItem } from '../cloudstorage-telegram/CloudStorage.ts';

const AccessTokenUpdate = async () => {

    const refreshTokens: string | undefined = await StorageGetItemAsync("RefreshToken");

    if (refreshTokens !== undefined) {
        try {
            await handleRefreshTokenUpdate(refreshTokens);
            return true;
        }
        catch (e) {
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