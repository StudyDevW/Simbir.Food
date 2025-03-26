import WebApp from "@twa-dev/sdk";
import { useEffect, useRef, useState } from "react";
import { useNavigate, useLocation, data } from 'react-router-dom';
import { GetMeInfo } from "../api-integrations/Interfaces/API_Interfaces";

const ElementMenu: React.FC<{name_element: string, icon_url: string, description?: string, is_mobile: boolean, onClickEx: () => void }> = ({name_element, icon_url, description, is_mobile, onClickEx}) => {
    return (<>
        <div className="app_profile_elements" onClick={onClickEx}>
            
            <div className="app_profile_elements_icon" style={{
                backgroundImage: `url(${icon_url})`
            }}></div>

            {is_mobile && <div className="app_profile_elements_name mobile">{name_element}</div>}

            {!is_mobile && <div className="app_profile_elements_name">{name_element}</div>}

            {description !== undefined && <>

                {is_mobile && <div className="app_profile_elements_desc mobile">{description}</div>}

                {!is_mobile && <div className="app_profile_elements_desc">{description}</div>}

            </>
            }

        </div>
    </>)
}

const ProfilePage: React.FC<{info: GetMeInfo, isMobile: boolean, onChange: (newValue: boolean) => void}> = ({info, isMobile, onChange}) => {

    const [closedProfile, setClosedProfile] = useState<boolean>(false);

    const [balanceUp, setBalanceUp] = useState<boolean>(false);

    const [balanceValue, setBalanceValue] = useState<number>(100);

    const inputRef = useRef<HTMLInputElement>(null);

    const handleFocusInput = () => {
        if (inputRef.current && isMobile) {
          inputRef.current.focus();
        }
    };

    const navigate = useNavigate();

    useEffect(() => {
        WebApp.disableVerticalSwipes();
    }, [])
    
    useEffect(()=>{
        if (balanceUp) {
            handleFocusInput();
        }
    }, [balanceUp])

    const handleTouchStart = (e: React.TouchEvent<HTMLDivElement>) => {

        if (!isMobile)
            return;

        const startX = e.touches[0].clientX;

        const handleTouchMove = (e: TouchEvent) => {
        const moveX = e.touches[0].clientX;
        const diffX = startX - moveX;

        if (Math.abs(diffX) > 110) {
            if (diffX < 0) {
                setClosedProfile(true);
            } 

            document.removeEventListener('touchmove', handleTouchMove);
        }
        };

        document.addEventListener('touchmove', handleTouchMove);
    };



    const RoleOutput = (roles: string[]) => {
        if (roles.includes("Admin"))
            return "Администратор";

        if (roles.includes("Courier"))
            return "Курьер";
  
        if (roles.includes("Client"))
            return "Клиент";     
    }

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

    const getOrderString = (count: number): string => {
        const lastDigit = count % 10;
        const lastTwoDigits = count % 100;
      
        if (lastDigit === 1 && lastTwoDigits !== 11) {
          return `${count} заказ`;
        } else if (
          (lastDigit >= 2 && lastDigit <= 4) &&
          (lastTwoDigits < 12 || lastTwoDigits > 14)
        ) {
          return `${count} заказа`;
        } else {
          return `${count} заказов`;
        }
    };
      
    const [isKeyboardVisible, setKeyboardVisible] = useState(false);

    return (<>

        {closedProfile && 
            <>
                <div className="app_profile_area closed" 
                    onAnimationEnd={(e)=>{
                        if (e.animationName === "profile_close_background")
                            onChange(false)
                    }}>

                    <div className="app_profile_area_panel closed">

                    </div>
                </div>
            </>
        }
        
        {!closedProfile &&
            <>
                <div className="app_profile_area" onTouchStart={handleTouchStart}>
                    <div className="app_profile_area_panel" style={isMobile ? {
                    height: `calc(100vh - 100px - 48px)`
                } : {height: `calc(100vh - 5px)`}}  
                onMouseLeave={()=> { if (!balanceUp) setClosedProfile(true) }}>
                        <div className="app_profile_info_area">
                            <div className="app_profile_info_avatar" style={{
                                backgroundImage: `url(${info.photo_url})`
                            }}></div>

                            <div className="app_profile_info_name_area">

                                {info.last_name === "" && <>
                                    <div className="app_profile_info_name">{`${info.first_name}`}</div>
                                    
                                    <div className="app_profile_info_role">
                                        <div className="app_profile_info_role_text">
                                            {RoleOutput(info.roles)}
                                        </div>
                                    </div>

                                </>}


                                {info.last_name !== "" && <>
                                    <div className="app_profile_info_name">{`${info.first_name}`}</div>
                                    
                                    <div className="app_profile_info_name small">{`${info.last_name}`}</div>

                                    <div className="app_profile_info_role marginedout">
                                        <div className="app_profile_info_role_text small">
                                            {RoleOutput(info.roles)}
                                        </div>
                                    </div>

                                </>}
                            </div>


                        </div>

                        <div className="app_profile_elements_address">
                            <div className="app_maincontent_balance_image">

                            </div>

                            <div className="app_maincontent_balance_title">
                                {`${info.money_value} руб.`}
                            </div>

                            <div className="app_profile_elements_desc"  onClick={()=> { handleFocusInput(); setBalanceUp(true); }}>{`Пополнить`}</div>
                        </div>

                        <div className="app_profile_elements_separator">{`Основное`}</div>

                        <ElementMenu 
                            is_mobile={isMobile} 
                            name_element="Корзина" 
                            description={`${getProductLabel(info.basket_items)}`} 
                            icon_url="./images/basket_icon.png"
                            onClickEx={()=> 
                                {
                                    info.basket_items > 0 ? 
                                    navigate("/basket") : 
                                    WebApp.showAlert("В корзину вы можете добавить блюда, чтобы потом их заказать")
                                }}/>
                        


                        <ElementMenu 
                            is_mobile={isMobile} 
                            name_element="Заказы" 
                            description={info.orders_count > 0 ? `${getOrderString(info.orders_count)}` : "Отсутствуют"} 
                            icon_url="./images/orders_icon.png"
                            onClickEx={()=> 
                                {
                                    info.orders_count > 0 ? 
                                    navigate("/ordersinfo") : 
                                    WebApp.showAlert("Здесь будут отображаться заказы")
                                }}/>

                      

                        {info.restaurant_own !== null && info.restaurant_own?.length > 0 && <>
                        
                            <div className="app_profile_elements_separator">{`Ресторанам`}</div>

                            <ElementMenu 
                                is_mobile={isMobile} 
                                name_element="Управление" 
                                description="" 
                                icon_url=""/>


                            <ElementMenu 
                                is_mobile={isMobile} 
                                name_element="Заказы" 
                                description="Отсутствуют" 
                                icon_url="./images/orders_icon_rests.png"/>



                        </>}

                        {info.roles.includes("Courier") && <>
                            <div className="app_profile_elements_separator">{`Курьеру`}</div>

                            <ElementMenu 
                                is_mobile={isMobile} 
                                name_element="Заказы" 
                                description="Отсутствуют" 
                                icon_url="./images/orders_icon_courier.png"/>
                        </>}

                        {info.roles.includes("Admin") && <>
                            <div className="app_profile_elements_separator">{`Администратору`}</div>

                            <ElementMenu 
                                is_mobile={isMobile} 
                                name_element="Пользователи" 
                                description="Перейти" 
                                icon_url="./images/users_icon.png"/>

                            <ElementMenu 
                                is_mobile={isMobile} 
                                name_element="Заявки" 
                                description="Перейти" 
                                icon_url="./images/cv-form_icon.png"/>

                            <ElementMenu 
                                is_mobile={isMobile} 
                                name_element="Рестораны" 
                                description="Перейти" 
                                icon_url="./images/restaurants_icon.png"/>

                        </>}

                        <div className="app_profile_elements_separator">{`Дополнительно`}</div>

                        <ElementMenu 
                            is_mobile={isMobile} 
                            name_element="Открыть точку" 
                            description="" 
                            icon_url="./images/request_icon_rest.png"/>
                        

                        {!info.roles.includes("Courier") && <> 
                            <ElementMenu 
                                is_mobile={isMobile} 
                                name_element="Стать курьером" 
                                description="" 
                                icon_url="./images/request_icon_courier.png"/>
                        </>}

                    </div>
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

                        <div className="app_balance_up_button" onClick={()=>navigate("/payment", { state: { money_to_up: balanceValue } })}>
                            Перейти к оплате
                        </div>
                      
                    </div>
                    
                </>}
            </>
        }
    </>)
}

export default ProfilePage;