


export interface GetMeInfo {
    Id: string,
    telegram_id: number,
    first_name: string,
    last_name: string | null,
    username: string | null,
    photo_url: string | null,
    chat_id: number,
    address: string | null,
    restaurant_own: string[] | null,
    money_value: number,
    roles: string[]
}

export interface AuthComponent {
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

export interface GetBasketInfo {
    basketInfo: {
        count: number,
        totalPrice: number
    },
    basketItem: {
        id: string,
        restaurant_id: string,
        name: string,
        price: number,
        image: string,
        weight: number,
        calories: number
    }[]
}