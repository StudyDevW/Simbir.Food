import WebApp from "@twa-dev/sdk";

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

class telegramUser {
    internalData: AuthComponent;

    constructor(initData: any, platform: boolean) {
        this.internalData = {
            id: initData?.id,
            first_name: initData?.first_name,
            last_name: initData?.last_name === undefined ? null : initData?.last_name,
            username: initData?.username === undefined ? null : initData?.username,
            is_bot: initData?.is_bot === undefined ? false : initData?.is_bot,
            photo_url: initData?.photo_url === undefined ? null : initData?.photo_url,
            chat_id: initData?.id,
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

export { telegramUser } 