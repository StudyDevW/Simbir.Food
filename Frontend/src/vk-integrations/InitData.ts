// import WebApp from "@twa-dev/sdk"; //Порт под ВК (study)

import vkBridge from '@vkontakte/vk-bridge';

import { AuthComponent } from '../api-integrations/Interfaces/API_Interfaces.ts';

class vkUser { 
    internalData: AuthComponent;

    constructor(initData: any, platform: boolean) {
        this.internalData = {
            vk_id: initData?.id,
            first_name: initData?.first_name,
            last_name: initData?.last_name === undefined ? null : initData?.last_name,
            // username: initData?.username === undefined ? null : initData?.username,
            // is_bot: initData?.is_bot === undefined ? false : initData?.is_bot,
            photo_max_orig: initData?.photo_max_orig === undefined ? null : initData?.photo_max_orig,
            // chat_id: initData?.id,
            address: "",
            device: platform ? "Mobile" : "PC",
            roles: ["Client"]
        };
    }

    public AuthData = (): AuthComponent => {
        return this.internalData;
    }

    public SetAddress = (address: string) => {
        this.internalData.address = address;
    }

    public GetAddress = () => {
        return this.internalData.address;
    }

    public GetDevice = () => {
        return this.internalData.device;
    }
}

const getUserData = async () => {
    try {
        const user = await vkBridge.send('VKWebAppGetUserInfo');
        return user;
    } catch (error) {
        console.error('Ошибка получения данных пользователя:', error);
        return null;
    }
};

const initVKApp = async () => {
    await vkBridge.send('VKWebAppInit');
    const user = await getUserData();
    const platform = await vkBridge.send('VKWebAppGetClientVersion');
    
    return {
        user,
        isMobile: platform.platform === 'android' || platform.platform === 'ios'
    };
};


export { vkUser, getUserData, initVKApp } 