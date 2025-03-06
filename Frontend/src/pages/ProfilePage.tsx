import WebApp from "@twa-dev/sdk";
import { useEffect, useState } from "react";
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

    const navigate = useNavigate();

    useEffect(() => {
        WebApp.disableVerticalSwipes();
    }, [])
    
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
                } : {height: `calc(100vh + 15px)`}}  
                onMouseLeave={()=>setClosedProfile(true)}>
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
                        </div>

                        <div className="app_profile_elements_separator">{`Основное`}</div>

                        <ElementMenu 
                            is_mobile={isMobile} 
                            name_element="Корзина" 
                            description="0 товаров" 
                            icon_url="./images/basket_icon.png"
                            onClickEx={()=>navigate("/basket")}/>
                        

                        <ElementMenu 
                            is_mobile={isMobile} 
                            name_element="Заказы" 
                            description="Отсутствуют" 
                            icon_url="./images/orders_icon.png"/>

                        {info.restaurant_own !== null && <>
                        
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

                    </div>
                </div>
            </>
        }
    </>)
}

export default ProfilePage;