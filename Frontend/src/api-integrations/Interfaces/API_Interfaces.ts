


export interface GetMeInfo {
    id: string,
    telegram_id: number,
    first_name: string,
    last_name: string | null,
    username: string | null,
    photo_url: string | null,
    chat_id: number,
    address: string | null,
    restaurant_own: string[] | null,
    basket_items: number,
    orders_count: number,
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

export interface OrderInfo {
    order_id: string,
    status_order: string,
    price_order: number,
    order_date: string,
    last_status_change: string,
    courier_info: {
        courier_id: string,
        user_id: string,
        car_number: string,
        address: string,
        chat_id: string,
        first_name: string,
        last_name: string,
        photo_url: string,
        username: string
    } | null,
    restaurant_info: {
        restaurant_id: string,
        restaurantName: string,
        address: string,
        phone_number: string,
        imagePath: string
    },
    food_items: {
        restaurant_id: string,
        name: string,
        price: number,
        image: string,
        weight: number,
        calories: number
    }[],
    client_address: string
}

export interface PaymentInfo {
    user_id: string,
    card_number: string,
    cvv: string,
    money_value: number,
    link_card: boolean
}

export interface RestaurantInfo {
    id: string,
    userId: string,
    restaurantName: string,
    address: string,
    phone_number: string,
    status: number,
    description: string,
    imagePath: string,
    open_time: string,
    close_time: string,
    average_mark: number
}

export interface FoodItemInfo {
    id: string,
    restaurant_id: string,
    name: string,
    price: number,
    image: string,
    weight: number,
    calories: number
}

export interface RequestClientInfo {
    Id: number;
    first_name: string;
    last_name: string;
    address: string;
    chat_id: number;
    username: string;
    photo_url: string;
    roles: string[];
}

export interface RequestInfo_Restaurants {
    request_id: number;
    restaurantName: string;
    address: string;
    phone_number: string;
    description: string;
    imagePath: string;
    open_time: string;
    close_time: string;
    request_description: string;
    request_time_add: string;
    client_info: RequestClientInfo;
}

export interface RequestInfo_Couriers {
    request_id: number;
    car_number: string;
    request_description: string;
    request_time_add: string;
    client_info: RequestClientInfo;
}

export interface RequestsGetAll {
    restaurant_requests: RequestInfo_Restaurants[];
    courier_requests: RequestInfo_Couriers[];
}

export interface ClientInfo {
    id: number;
    first_name: string;
    last_name: string;
    username: string;
    photo_url: string;
    roles: string[];
    orders_count: number;
    money_value: number;
}

export interface ClientSelectionSettings {
    from: number;
    count: number;
}

export interface ClientGetAll {
    settings: ClientSelectionSettings;
    content: ClientInfo[];
}

export interface RestaurantsInfoForOwner {
    id: string,
    restaurantName: string,
    address: string,
    status: number,
    imagePath: string
}

export interface OrderForCourierDto
{
    orderId: string,
    restaurantId: string,
    restaurantName: string,
    restaurantAddress: string,
    phoneNumber: string,
    clientAddress: string,
    photoUrl: string,
    clientFirstName: string,
    clientSecondName: string,
    orderDate: string,
    statusOrder: number
}
