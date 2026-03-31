import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import WebApp from "@twa-dev/sdk";
import { handleCourierRequest } from "../../api-integrations/RequestAPI";
import { StorageGetItem } from "../../vk-integrations/cloudstorage/CloudStorage";
import { BackButton } from '@twa-dev/sdk/react';

const CourierRequestPage: React.FC = () => {
    const [carNumber, setCarNumber] = useState<string>("");
    const [description, setDescription] = useState<string>("");
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
        const accessToken = await StorageGetItem('AccessToken');

        if (!accessToken || accessToken === "empty") {
            WebApp.showAlert("Ошибка: нет токена авторизации.");
            return;
        }

        const requestCreated = await handleCourierRequest(carNumber, description, accessToken);

        if (requestCreated) {
            WebApp.showAlert("Заявка успешно отправлена!");
            navigate("/");
        } else {
            WebApp.showAlert("Ошибка при отправке заявки.");
        }
    };

    return (<>
        <BackButton onClick={()=>navigate("/")}/>
        <div className="app_background_area">
        
            <div className="app_layout_area" style={ isMobile ? { marginTop: '100px' } : {}}>

                <div className="request-page">
                    <div className="form-container">
                        <input
                            type="text"
                            placeholder="Номер машины"
                            value={carNumber}
                            onChange={(e) => setCarNumber(e.target.value)}
                        />
                        <textarea
                            placeholder="Описание заявки"
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
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
    </>);
};

export default CourierRequestPage;
