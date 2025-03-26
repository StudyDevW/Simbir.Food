import { useEffect, useRef, useState } from 'react'
import '../styles/AppStyle.sass'
import WebApp from '@twa-dev/sdk';
import { useNavigate, useLocation, data } from 'react-router-dom';
import { StorageGetItem } from '../telegram-integrations/cloudstorage/CloudStorage';
import { BackButton } from '@twa-dev/sdk/react';
import { GetBasketInfo, GetMeInfo } from '../api-integrations/Interfaces/API_Interfaces';
import { handleGetInfoMe } from '../api-integrations/ClientInfoAPI';
import { handleGetBasketInfo } from '../api-integrations/BasketAPI';

import { YMaps, Map, Placemark, GeolocationControl } from '@pbe/react-yandex-maps';
import { handleOrderCreate } from '../api-integrations/OrderAPI';

var YANDEX_API_KEY = import.meta.env.VITE_YANDEX_API_KEY;

const OrderPage: React.FC = () => {

    const navigate = useNavigate();

    const [isMobile, setIsMobile] = useState<boolean>(false);
    
    const [userInfo, setUserInfo] = useState<GetMeInfo | null>(null);
  
    const [basketInfo, setBasketInfo] = useState<GetBasketInfo | null>(null);

    const [location, setLocation] = useState<[number, number] | null>(null);

    const [balanceUp, setBalanceUp] = useState<boolean>(false);

    const [balanceValue, setBalanceValue] = useState<number>(100);
    
    const inputRef = useRef<HTMLInputElement>(null);

    const handleFocusInput = () => {
        if (inputRef.current && isMobile) {
          inputRef.current.focus();
        }
    };


    useEffect(()=>{
        WebApp.setHeaderColor('#EAEAEA');

        WebApp.setBackgroundColor('#004681');
    
        if (WebApp.platform === 'ios' || WebApp.platform === 'android')
          setIsMobile(true);
        else 
          setIsMobile(false);
    
        WebApp.ready();
    
        ProfileGet();
    }, [])
    
    useEffect(()=>{
        if (userInfo !== null && userInfo.address !== null) {
            getCoordinates(userInfo.address);
        }
    }, [userInfo])

    useEffect(()=>{
        if (basketInfo === null) {
            BasketGet();
        }
    }, [basketInfo])


    const GetUserRequestAPI = async (accessToken: string) => {
    
        const getuser = await handleGetInfoMe(accessToken);
        
        if (getuser !== null) {
          setUserInfo(getuser);
        }
    }

    const GetBasketRequestAPI = async (accessToken: string) => {
        const getbasket = await handleGetBasketInfo(accessToken);

        if (getbasket !== null) {
            setBasketInfo(getbasket);
        }
    }

    const GetOrderCreateRequestAPI = async (accessToken: string) => {
        const orderState = await handleOrderCreate(accessToken);

        if (orderState) {
            navigate("/");
        }
    }

    const ProfileGet = async () => {
        const accessToken: string = await StorageGetItem('AccessToken');
    
        if (accessToken !== "empty") {
          await GetUserRequestAPI(accessToken);
        }
    }

    const BasketGet = async () => {
        const accessToken: string = await StorageGetItem('AccessToken');
    
        if (accessToken !== "empty") {
          await GetBasketRequestAPI(accessToken);
        }
    }

    const OrderCreate = async () => {
        const accessToken: string = await StorageGetItem('AccessToken');
    
        if (accessToken !== "empty") {
          await GetOrderCreateRequestAPI(accessToken);
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

    const getCoordinates = async (address: string) => {
        try {
          const response = await fetch(`https://geocode-maps.yandex.ru/1.x/?apikey=${YANDEX_API_KEY}&geocode=${encodeURIComponent(`Ульяновск, ${address}`)}&format=json`);
          const data = await response.json();
    
          if (data.response.GeoObjectCollection.featureMember.length > 0) {
            const [lon, lat] = data.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos.split(' ').map(Number);
     
            setLocation([lat, lon]);
          } else {
            alert('Адрес не найден');
          }
        } catch (error) {
            alert('Ошибка при получении координат');
        }
    };

    const getProductLabel = (count: number): string => {
        if (count % 10 === 1 && count % 100 !== 11) {
          return `${count} товар`;
        } else if (
          (count % 10 >= 2 && count % 10 <= 4) &&
          (count % 100 < 10 || count % 100 >= 20)
        ) {
          return `${count} товара`;
        } else {
          return `${count} товаров`;
        }
    };

    return (<>
        <BackButton onClick={()=>navigate("/basket")}/>

        <div className="app_background_area">


            <div className="app_layout_area" style={ isMobile ? { marginTop: '100px' } : {}}>

            {userInfo === null && LoadingDraw()}

            {userInfo !== null && <>
                
                <div className="app_preorder_area" style={isMobile ? {height: 'calc(100% - 100px - 10px)'} : {}}>

                    <div className="app_preorder_maparea">
                        
                        <div className="app_delivery_header_image" style={{marginLeft: '6px'}}></div>

                        {userInfo.address !== null && 
                            <>
                                <div className="app_delivery_header_title_area" onClick={()=>navigate("/address_select", { state: { address_default: userInfo.address, flag: "orderCreate" } })}>
                                    <div className="app_delivery_header_title">{userInfo.address}</div>
                                    <div className="app_delivery_header_title big">Адрес доставки</div>
                                </div>

                            
                                
                                <div className="app_preorder_maparea_second">
                                        {location !== null &&
                                         <YMaps>
                                            <Map
                                            defaultState={{ center: location || [54.314194, 48.419610], zoom: 17 }} 
                                            width="100%" height="115%">
                                            {<Placemark geometry={location} options={{
                                            // Options. You must specify this type of layout.
                                            iconLayout: 'default#image',
                                            // Custom image for the placemark icon.
                                            iconImageHref: "../../images/location.png",
                                            // The size of the placemark.
                                            iconImageSize: [40, 40],
                                            // The offset of the upper left corner of the icon relative
                                            // to its "tail" (the anchor point).
                                            iconImageOffset: [-18, -42]}}/>
                                            }
                                            </Map>
                                        </YMaps>
                                        }
                              
                                    
                                </div>
                        </>}

                        {userInfo.address === null && 
                        <>
                            <div className="app_delivery_header_title_area" onClick={()=>navigate("/address_select")}>
                                <div className="app_delivery_header_title big" style={{
                                    marginTop: '15px'
                                }}>{`Укажите адрес доставки`}</div>
                            </div>
                        </>
                        }
                    </div>

                    <div className="app_preorder_miniitemarea">
                        <div className="app_preorder_basket_image"></div>
                        
                        {basketInfo !== null && <>
                            <div className="app_order_miniitem_title_area">
                                <div className="app_delivery_header_title">Заказ из корзины</div>
                                <div className="app_delivery_header_title big">
                                    {`${getProductLabel(basketInfo.basketInfo.count)} на ${basketInfo.basketInfo.totalPrice} руб`}
                                </div>
                            </div>
                        </>}

                        

                    </div>

                    
                    <div className="app_preorder_miniitemarea">
                        <div className="app_preorder_ruble_image"></div>

                        <div className="app_order_miniitem_title_area">
                                <div className="app_delivery_header_title">Ваш текущий баланс</div>
                                <div className="app_delivery_header_title big">
                                    {`${userInfo.money_value} руб`}
                                </div>
                        </div>

                    </div>

                    {basketInfo !== null && userInfo.money_value >= basketInfo.basketInfo.totalPrice &&
                      <div className="app_preorder_paybutton" 
                      onClick={OrderCreate}>
                          {`Оплатить`}
                      </div>
                    }

                    {basketInfo !== null && userInfo.money_value < basketInfo.basketInfo.totalPrice &&
                      <div className="app_preorder_paybutton" onClick={()=> setBalanceUp(true) }>
                          {`Пополнить баланс`}
                      </div>
                    }


                </div>
            
                {balanceUp && <>
                    <div className="balance_popup_area" style={isMobile ? { animation: 'none', bottom: '430px' } : {}} onMouseLeave={()=>setBalanceUp(false)}>
                        <div className="app_maincontent_title">Укажите сумму</div>

                        <div className="app_maincontent_searchbar_decor" style={{marginBottom: "20px"}}>
                                <input className='app_maincontent_searchbar'
                                    onBlur={isMobile? () => handleFocusInput() : () => {}}
                                    type="number"
                                    value={balanceValue}
                                    ref={inputRef}
                                    // onFocus={() => setKeyboardFocused(true)}
                                    // onBlur={() => 
                                    // { 
                                    //     if (inputValue === "")
                                    //         setKeyboardFocused(false);
                                    // }}
                                    onChange={(e) => setBalanceValue(Number(e.target.value))}
                                    placeholder={'Сумма для пополнения'}
                                /> 

                                <div className="app_maincontent_bar_ruble" 
                                style={{backgroundImage: './images/icon-ruble.png'}}></div>

                                
                        </div>

                        <div className="app_balance_up_button" onClick={()=>navigate("/payment", { state: { money_to_up: balanceValue, redirect_to_order: true } })}>
                            Перейти к оплате
                        </div>
                      
                    </div>
                    
                </>}

            </>}

            </div>        
        </div>

    </>)
}

export default OrderPage;