import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import WebApp from "@twa-dev/sdk";
import { StorageGetItem } from "../../telegram-integrations/cloudstorage/CloudStorage";
import { handleRestaurantRequest } from "../../api-integrations/RequestAPI";

const RestaurantRequestPage: React.FC = () => {
    const [restaurantName, setRestaurantName] = useState<string>("");
    const [address, setAddress] = useState<string>("");
    const [phoneNumber, setPhoneNumber] = useState<string>("");
    const [description, setDescription] = useState<string>("");
    const [imagePath, setImagePath] = useState<string>("");
    const [openTime, setOpenTime] = useState<string>("");
    const [closeTime, setCloseTime] = useState<string>("");
    const [requestDescription, setRequestDescription] = useState<string>("");
    const [isMobile, setIsMobile] = useState<boolean>(false);
    const navigate = useNavigate();

    useEffect(()=>{
                WebApp.setHeaderColor('#EAEAEA');
        
                WebApp.setBackgroundColor('#004681');
            
                if (WebApp.platform === 'ios' || WebApp.platform === 'android')
                  setIsMobile(true);
                else 
                  setIsMobile(false);
            
                WebApp.ready();
            }, [])

    const handleSubmit = async () => {
        const accessToken = await StorageGetItem("AccessToken");

        if (!accessToken || accessToken === "empty") {
            WebApp.showAlert("Ошибка: нет токена авторизации.");
            return;
        }

        const requestCreated = await handleRestaurantRequest(
            restaurantName,
            address,
            phoneNumber,
            description,
            imagePath,
            openTime,
            closeTime,
            requestDescription,
            accessToken
        );

        if (requestCreated) {
            WebApp.showAlert("Заявка успешно отправлена!");
            navigate("/");
        } else {
            WebApp.showAlert("Ошибка при отправке заявки.");
        }
    };

    return (

        <div className="app_background_area">
        
            <div className="app_layout_area" style={ isMobile ? { marginTop: '100px' } : {}}>

            <div className="request-page">
                <h2>Регистрация ресторана</h2>
                <div className="form-container">
                    <input
                        type="text"
                        placeholder="Название ресторана"
                        value={restaurantName}
                        onChange={(e) => setRestaurantName(e.target.value)}
                    />
                    <input
                        type="text"
                        placeholder="Адрес"
                        value={address}
                        onChange={(e) => setAddress(e.target.value)}
                    />
                    <input
                        type="text"
                        placeholder="Номер телефона"
                        value={phoneNumber}
                        onChange={(e) => setPhoneNumber(e.target.value)}
                    />
                    <textarea
                        placeholder="Описание"
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                    />
                    <input
                        type="text"
                        placeholder="Ссылка на изображение"
                        value={imagePath}
                        onChange={(e) => setImagePath(e.target.value)}
                    />
                    <input
                        type="time"
                        placeholder="Время открытия"
                        value={openTime}
                        onChange={(e) => setOpenTime(e.target.value)}
                    />
                    <input
                        type="time"
                        placeholder="Время закрытия"
                        value={closeTime}
                        onChange={(e) => setCloseTime(e.target.value)}
                    />
                    <textarea
                        placeholder="Дополнительная информация"
                        value={requestDescription}
                        onChange={(e) => setRequestDescription(e.target.value)}
                    />
                    <div className="button-group">
                        <button className="back-btn" onClick={() => navigate(-1)}>Назад</button>
                        <button className="submit-btn" onClick={handleSubmit}>Отправить заявку</button>
                    </div>
                </div>
            </div>

                {(isMobile) && <div className="app_mobile_footer" style={{zIndex: '15'}}>Симбир Еда</div>}

            </div>
        
        </div>

    );
};

export default RestaurantRequestPage;
