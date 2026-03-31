import { BackButton } from '@twa-dev/sdk/react';
import { useEffect, useState } from 'react'
import '../styles/AppStyle.sass'
import WebApp from '@twa-dev/sdk';
import { useNavigate, useLocation, data } from 'react-router-dom';
import { YMaps, Map, Placemark, GeolocationControl } from '@pbe/react-yandex-maps';
import { GetMeInfo, OrderInfo } from '../api-integrations/Interfaces/API_Interfaces';
import { handleGetInfoMe } from '../api-integrations/ClientInfoAPI';
import { StorageGetItem } from '../vk-integrations/cloudstorage/CloudStorage';
import { handleOrdersGet } from '../api-integrations/OrderAPI';
import { handleLoadImage } from '../api-integrations/ImageAPI';

var YANDEX_API_KEY = import.meta.env.VITE_YANDEX_API_KEY;

const BasketItem: React.FC<{imageLink: string, name: string, price: number }> = ({imageLink, name, price}) => {

    const [imageRendered, setImageRendered] = useState<string | null>(null);

    const renderImage = async () => {
        const accessToken: string = await StorageGetItem('AccessToken');

        if (accessToken !== "empty") {
            const imageItem = await handleLoadImage(accessToken, imageLink);

            if (imageItem !== null)
                setImageRendered(imageItem);
        }
    }

    useEffect(() => {
        renderImage();

    }, [])

    return (<>
             <div className="app_preorder_miniitemarea" style={{backgroundColor: '#EAEAEA', height: '70px'}}>
                <div className="app_basket_item_image" style={{
                    backgroundImage: `url(${imageRendered})`
                }}>

                </div>

                <div className="app_basket_item_title">
                    {`${name}`}
                </div>

                <div className="app_basket_item_title sm">
                    {`${price} руб.`}
                </div>

            </div>
        
    </>)
}


const OrderItem: React.FC<{info: OrderInfo, onClick: () => void }> = ({info, onClick}) => {


    // const getProductLabel = (count: number): string => {
    //     if (count % 10 === 1 && count % 100 !== 11) {
    //       return `${count} товар`;
    //     } else if (
    //       (count % 10 >= 2 && count % 10 <= 4) &&
    //       (count % 100 < 10 || count % 100 >= 20)
    //     ) {
    //       return `${count} товара`;
    //     } else {
    //       return `${count} товаров`;
    //     }
    // };

    const DatePrint = (dateFrom: string) => {
        const date = new Date(dateFrom);

        const day = date.getDate();
        const month = date.toLocaleString('russian', { month: '2-digit' }); // Получаем название месяца
        const year = date.getFullYear();

        return `Заказ от ${day < 10 ? `0${day}` : day}.${month}.${year}`;
    }

    return (<>
         <div className="app_order_item" onClick={onClick}>

                <div className="app_preorder_box_image"></div>

                <div className="app_order_item_title">
                    {`${DatePrint(info.order_date)}`}
                </div>

                <div className="app_order_item_title sm">
                    {`${info.status_order}`}
                </div>
         </div>
    </>)
}

const OrderWithMap: React.FC<{info: OrderInfo}> = ({info}) => {

    const [imageAvatar, setImageAvatar] = useState<string>("");
    const [isMobile, setIsMobile] = useState<boolean>(false);

    const [locationRestaurant, setLocationRestaurant] = useState<[number, number] | null>(null);
    const [locationClient, setLocationClient] = useState<[number, number] | null>(null);
    const [locationCourier, setLocationCourier] = useState<[number, number] | null>(null);

    useEffect(()=>{
        if (WebApp.initDataUnsafe.user?.photo_url !== undefined) {
            setImageAvatar(WebApp.initDataUnsafe.user?.photo_url);
        }

        if (WebApp.platform === 'ios' || WebApp.platform === 'android')
            setIsMobile(true);
        else 
            setIsMobile(false);

        getCoordinates(info.restaurant_info.address, "rest");

        getCoordinates(info.client_address, "client");
    },[])

    const getCoordinates = async (address: string, type: string) => {
        try {
          const response = await fetch(`https://geocode-maps.yandex.ru/1.x/?apikey=${YANDEX_API_KEY}&geocode=${encodeURIComponent(`Ульяновск, ${address}`)}&format=json`);
          const data = await response.json();
    
          if (data.response.GeoObjectCollection.featureMember.length > 0) {
            const [lon, lat] = data.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos.split(' ').map(Number);
     
            if (type === "rest") 
                setLocationRestaurant([lat, lon]);

            if (type === "client")
                setLocationClient([lat, lon]);

            if (type === "courier")
                setLocationCourier([lat, lon]);
          } else {
            alert('Адрес не найден');
          }
        } catch (error) {
            alert('Ошибка при получении координат');
        }
    };

    return (<>
        <div className="app_delivery_header" style={
        {
            backdropFilter: 'blur(8px)', 
            position: 'fixed', 
            background: 'linear-gradient(#EAEAEA, transparent)',
            zIndex: '5',
            maxWidth: '1000px'}
        }>

                
            <div className="app_delivery_header_image"></div>


            {info.client_address !== null && 
            <>
                <div className="app_delivery_header_title_area">
                    <div className="app_delivery_header_title">{info.client_address}</div>
                    <div className="app_delivery_header_title big">Адрес доставки</div>
                </div>
            </>}


            <div className="app_delivery_header_profile"
            style={{
                backgroundImage: `url(${imageAvatar})`,
            }}></div>
        </div>


        <YMaps>
            <Map 
            state={{center: locationRestaurant || [54.314194, 48.419610], zoom: 15}} 
            width="100%" height="60%">
            
                <Placemark geometry={locationRestaurant!} options={{
                // Options. You must specify this type of layout.
                iconLayout: 'default#image',
                // Custom image for the placemark icon.
                iconImageHref: "../../images/location_rest.png",
                // The size of the placemark.
                iconImageSize: [40, 40],
                // The offset of the upper left corner of the icon relative
                // to its "tail" (the anchor point).
                iconImageOffset: [-18, -42]}}/>

                <Placemark geometry={locationClient!} options={{
                // Options. You must specify this type of layout.
                iconLayout: 'default#image',
                // Custom image for the placemark icon.
                iconImageHref: "../../images/location.png",
                // The size of the placemark.
                iconImageSize: [40, 40],
                // The offset of the upper left corner of the icon relative
                // to its "tail" (the anchor point).
                iconImageOffset: [-18, -42]}}/>
            
            </Map>
        </YMaps>

        <div className="app_maincontent" style={{height: '60%'}}>
            <div className="app_maincontent_title" style={{fontSize: '20px'}}>Информация о заказе</div>

            <div className="app_maincontent_area" style={isMobile ? {height: 'calc(100% - 200px)'} : {}}>

                <div className="app_preorder_area" style={isMobile ? {height: 'calc(100% - 100px - 10px)'} : {}}>

                    <div className="app_preorder_miniitemarea" style={{backgroundColor: 'black'}}>
                        <div className="app_preorder_box_image"></div>
             
                        <div className="app_order_miniitem_title_area">
                            <div className="app_delivery_header_title" style={{color: 'white'}}>
                                {`${info.status_order}`}
                            </div>
                            <div className="app_delivery_header_title big" style={{color: "rgb(200, 200, 200)"}}>
                                {`Статус заказа`}
                            </div>
                        </div>
                    </div>


                    <div className="app_order_items_separator_info">
                        {`Блюда в заказе`}
                    </div>

                    {info.food_items.map((item, index) => <>
                    <BasketItem 
                        key={index}
                        name={item.name} 
                        imageLink={item.image}
                        price={item.price}/>
                    </>)}
                </div>

               

            </div>
        </div>
    </>)
}

const OrderInfoPage: React.FC = () => {


    
    const navigate = useNavigate();

    const [isMobile, setIsMobile] = useState<boolean>(false);

    const [userInfo, setUserInfo] = useState<GetMeInfo | null>(null);

    const [detailsClicked, setDetailsClicked] = useState<boolean>(false);

    const [orderInformation, setOrdersInformation] = useState<OrderInfo[]>([]);
    
    const [selectedOrderId, setOrderId] = useState<string>("");

    useEffect(()=>{
        WebApp.setHeaderColor('#EAEAEA');

        WebApp.setBackgroundColor('#004681');
    
        if (WebApp.platform === 'ios' || WebApp.platform === 'android')
          setIsMobile(true);
        else 
          setIsMobile(false);
    
        WebApp.ready();
    
        ProfileGet();

        OrdersInfoGet();
       
    }, [])

    const GetUserRequestAPI = async (accessToken: string) => {
    
        const getuser = await handleGetInfoMe(accessToken);
        
        if (getuser !== null) {
          setUserInfo(getuser);
        }
    }

    const GetOrdersRequestAPI = async (accessToken: string) => {

        const ordersinfo = await handleOrdersGet(accessToken);

        if (ordersinfo !== null) {
            setOrdersInformation(ordersinfo);
        }
            
    }

    const OrdersInfoGet = async () => {
        const accessToken: string = await StorageGetItem('AccessToken');
    
        if (accessToken !== "empty") {
            await GetOrdersRequestAPI(accessToken);
        }
    }

    const ProfileGet = async () => {
        const accessToken: string = await StorageGetItem('AccessToken');
    
        if (accessToken !== "empty") {
          await GetUserRequestAPI(accessToken);
        }
    }

    const LoadingDraw = () => {
        return (<>
            <div className="app_loading_area" style={ isMobile ? { height: 'calc(100% - 100px - 45px)' } : {} }>
                <div className="app_loading_letter">
                    <div className="app_loading_bar"></div>
                </div>
            </div>
        </>)
    }


    return (<>

       {!detailsClicked && <BackButton onClick={()=>navigate("/")}/>}


       {detailsClicked && <BackButton onClick={()=>setDetailsClicked(false)}/>}

       <div className="app_background_area">

            <div className="app_layout_area" style={ isMobile ? { marginTop: '100px' } : {}}>

                {orderInformation.length <= 0 && LoadingDraw()}

                {(userInfo !== null && !detailsClicked) && 
                <>
                    <div className="app_maincontent" style={{height: '100%'}}>
                        <div className="app_maincontent_title">Заказы</div>

                        <div className="app_maincontent_area" style={isMobile ? {height: 'calc(100% - 200px)'} : {}}>

                            {orderInformation.map((item, index) => <>
                                <OrderItem key={index} info={item} onClick={()=> { setDetailsClicked(true); setOrderId(item.order_id); }}/>
                            </>)}

                        </div>
                    </div>

                </>}

                {(userInfo !== null && detailsClicked) && <>
                    <OrderWithMap info={orderInformation.find(index => index.order_id === selectedOrderId)!}/>
                </>}

            </div>
        </div>
    </>)
}

export default OrderInfoPage;