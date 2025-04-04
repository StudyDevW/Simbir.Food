import { use, useEffect, useState } from 'react'
import '../../styles/AppStyle.sass'
import WebApp from '@twa-dev/sdk';
import { YMaps, Map, Placemark, GeolocationControl } from '@pbe/react-yandex-maps';
import { useNavigate, useLocation } from 'react-router-dom';
import { handleUserAuth, handleUserRegister } from "../../api-integrations/AuthAPI.ts";
import { telegramUser } from '../../telegram-integrations/InitData.ts';
import { BackButton } from '@twa-dev/sdk/react';
import { AuthComponent, OrderForCourierDto } from '../../api-integrations/Interfaces/API_Interfaces.ts';
import { handleOrdersGet } from '../../api-integrations/OrderAPI.ts';
import { StorageGetItem } from '../../telegram-integrations/cloudstorage/CloudStorage.ts';
import { handleOrderAccept } from '../../api-integrations/CourierAPI.ts';

var YANDEX_API_KEY = import.meta.env.VITE_YANDEX_API_KEY;

var userData = new telegramUser(
    WebApp.initDataUnsafe.user, 
    (WebApp.platform === 'ios' || WebApp.platform === 'android')
);

const AddressPageCourier: React.FC = () => {

    const locationReact = useLocation();

    const orderInfo: OrderForCourierDto | null = locationReact.state?.orderInfo;
    
    

    const navigate = useNavigate();
    //Yandex Integrations
    const [inputValue, setInputValue] = useState('');

    const [suggestions, setSuggestions] = useState<string[]>([]);

    const [loading, setLoading] = useState(false);
    //!Yandex Integrations

    const [isMobile, setIsMobile] = useState<boolean>(false);

    const [clickedMaps, setClickedMaps] = useState<boolean>(false);

    const [isAuthOperated, setIsAuthOperated] = useState<boolean>(false);

    const [keyboardFocused, setKeyboardFocused] = useState<boolean>(false);

    const [imageAvatar, setImageAvatar] = useState<string>("");

    const [address, setAddress] = useState<string | null>(null);

    const [buttonUpdate, setButtonUpdate] = useState<boolean>(false);

    const [addressRest, setAddressRest] = useState<string | null>(null);

    const [location, setLocation] = useState<[number, number] | null>(null);

    const [locationNew, setLocationNew] = useState<[number, number] | null>(null);

    const [locationNewRest, setLocationNewRest] = useState<[number, number] | null>(null);

    const [locationRestaurant, setLocationRestaurant] = useState<[number, number] | null>(null);
    const [locationClient, setLocationClient] = useState<[number, number] | null>(null);
 

    useEffect(() => {
        WebApp.setHeaderColor('#EAEAEA');

        WebApp.setBackgroundColor('#004681');

        if (WebApp.platform === 'ios' || WebApp.platform === 'android')
            setIsMobile(true);
        else 
            setIsMobile(false);

        if (WebApp.initDataUnsafe.user?.photo_url !== undefined) {
            setImageAvatar(WebApp.initDataUnsafe.user?.photo_url);
        }

        WebApp.ready();

        if (orderInfo !== null) {
            getCoordinates(orderInfo.restaurantAddress, "rest");

            getCoordinates(orderInfo.clientAddress, "client");
        }

    }, []);

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

          } else {
            alert('Адрес не найден');
          }
        } catch (error) {
            alert('Ошибка при получении координат');
        }
    };

    useEffect(()=>{
        if (isMobile) {
            WebApp.lockOrientation();
            WebApp.requestFullscreen();
        }
    }, [isMobile])

    useEffect(()=>{
        if (keyboardFocused) {

        }
    }, [keyboardFocused])

    const UserAuthRequestAPI = async (authvars: AuthComponent) => {

        const validate = await handleUserAuth(authvars);

        if (validate) {
            navigate("/");
        }
    
    }

    useEffect(()=>{
        if (isAuthOperated) {
            if (address !== null)
                userData.SetAddress(address);

            UserAuthRequestAPI(userData.AuthData());
        }
    }, [isAuthOperated])

    const LoadingDraw = () => {
        return (<>
            <div className="app_loading_area" style={ isMobile ? { height: 'calc(100% - 100px - 45px)' } : {} }>
                <div className="app_loading_letter">
                    <div className="app_loading_bar"></div>
                </div>
            </div>
        </>)
    }

    const formatDate = (dateString: string) => {
        const options: Intl.DateTimeFormatOptions = {
            day: 'numeric',
            month: 'long',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        };
        return new Date(dateString).toLocaleDateString('ru-RU', options);
    };

    const handleAcceptOrder = async (orderId: string) => {
        if (window.confirm("Вы уверены, что хотите принять этот заказ?")) {
            try {
                const accessToken = await StorageGetItem("AccessToken");
                if (accessToken === "empty") return;
                
                // Здесь должен быть вызов API для принятия заказа
                // await acceptOrderApiCall(orderId, accessToken);

                await handleOrderAccept(accessToken, orderId);

                alert(`Заказ ${orderId} принят!`);
                navigate("/");
            } catch (error) {
                console.error("Ошибка при принятии заказа:", error);
                alert("Не удалось принять заказ");
            }
        }
    };

    

    useEffect(() => {
        if (buttonUpdate) {

            setButtonUpdate(false);
        }
    }, [buttonUpdate])

    return (
    <>

        {<BackButton onClick={()=>navigate("/")}/>}

       

        <div className="app_background_area">

            <div className="app_layout_area" style={ isMobile ? { marginTop: '100px' } : {}}>

                {!isAuthOperated && orderInfo !== null && 
                <>
                    <div className="app_delivery_header" style={
                    {
                        backdropFilter: 'blur(8px)', 
                        position: 'fixed', 
                        background: 'linear-gradient(#EAEAEA, transparent)',
                        zIndex: '5',
                        maxWidth: '1000px'}
                    }>

                        <div className="app_delivery_header_image"></div>

                        { 
                        <>
                            <div className="app_delivery_header_title_area">
                                <div className="app_delivery_header_title big" style={{
                                    marginTop: '15px'
                                }}>{`Заказ #${orderInfo.orderId.substring(0, 8)}`}</div>
                            </div>
                        </> 
                        }

                        <div className="app_delivery_header_profile"
                        style={{
                            backgroundImage: `url(${imageAvatar})`,
                        }}></div>
                    </div>


                    <YMaps>
                    

                        {locationClient && <>
                            <Map 
                                state={{center: locationClient || [54.314194, 48.419610], zoom: 15}} 
                                width="100%" height="80%">
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
                                
                            </Map>
                        </>}

                    </YMaps>

                    {(!keyboardFocused) && <>
                        <div className={"app_maincontent maps"} style={{height: '230px'}}>
                            <div className="app_maincontent_title" 
                            style={{fontSize: "20px"}}>{"О заказе"}</div>

                            <div className="app_courier_info_area">

                                <div className="app_courier_info_text">{`· Дата заказа: ${formatDate(orderInfo.orderDate)}`}</div>

                                <div className="app_courier_info_text">{`· Клиент: ${orderInfo.clientFirstName + " " + orderInfo.clientSecondName}`}</div>

                                <div className="app_courier_info_text">{`··· Адрес получения: ${orderInfo.restaurantAddress}`}
                                    <div className="app_courier_info_icon" 
                                    style={{backgroundImage: 'url("../../images/location_rest.png")'}}></div>
                                </div>

                                <div className="app_courier_info_text">{`··· Адрес доставки: ${orderInfo.clientAddress}`}
                                    <div className="app_courier_info_icon" 
                                    style={{backgroundImage: 'url("../../images/location.png")'}}></div>
                                </div>

                        
                            </div>

                            <div className="app_maincontent_address_button_complete" 
                                style={{
                                    position: 'absolute', 
                                    marginLeft: '10px', 
                                    width: 'calc(100% - 20px)', 
                                    height: '50px', 
                                    bottom: '10px',
                                    borderRadius: '15px'
                                }}
                                onClick={() => handleAcceptOrder(orderInfo.orderId)}>
                                    Принять       
                            </div>
                        </div>
                    </>} 
                </>}

                {(isAuthOperated && userData.GetAddress() !== "" && userData.GetDevice() !== "") && <>
                    {LoadingDraw()}
                </>}

                {(isMobile) && <div className="app_mobile_footer" style={{zIndex: '15'}}>Симбир Еда</div>}

            </div>

        </div>
        
    </>)
}

export default AddressPageCourier