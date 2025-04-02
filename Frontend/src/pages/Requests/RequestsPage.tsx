import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import WebApp from "@twa-dev/sdk";
import { 
    handleGetRequests,
    handleApproveRestaurantRequest,
    handleRejectRestaurantRequest,
    handleApproveCourierRequest,
    handleRejectCourierRequest
} from "../../api-integrations/RequestAPI";
import { StorageGetItem } from "../../telegram-integrations/cloudstorage/CloudStorage";
import { RequestsGetAll } from "../../api-integrations/Interfaces/API_Interfaces";
import '../../styles/AppStyle.sass'
import { BackButton } from '@twa-dev/sdk/react';

const RequestsPage: React.FC = () => {
    const [isMobile, setIsMobile] = useState<boolean>(false);
    const [requests, setRequests] = useState<RequestsGetAll | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [processingId, setProcessingId] = useState<number | null>(null);
    const navigate = useNavigate();

    useEffect(() => {
        WebApp.setHeaderColor('#EAEAEA');
        WebApp.setBackgroundColor('#004681');
        
        if (WebApp.platform === 'ios' || WebApp.platform === 'android') {
            setIsMobile(true);
        } else {
            setIsMobile(false);
        }
        
        WebApp.ready();
    }, []);

    const fetchRequests = async () => {
        setLoading(true);
        setError(null);
        
        try {
            const accessToken = await StorageGetItem("AccessToken");
            if (accessToken === "empty") {
                setError("Требуется авторизация");
                return;
            }
            
            const requestsData = await handleGetRequests(accessToken);
            if (requestsData) {
                setRequests(requestsData);
            } else {
                setError("Не удалось загрузить заявки");
            }
        } catch (err) {
            setError("Произошла ошибка при загрузке");
            console.error("Ошибка загрузки заявок:", err);
        } finally {
            setLoading(false);
        }
    };

    const handleRequestAction = async (
        requestId: number,
        isRestaurant: boolean,
        action: 'approve' | 'reject'
    ) => {
        setProcessingId(requestId);
        try {
            let accessToken = await StorageGetItem("AccessToken");
            if (accessToken === "empty") {
                setError("Требуется авторизация");
                return;
            }

            let success = false;
            let retry = true;

            while (retry) {
                retry = false;
                
                if (isRestaurant) {
                    success = action === 'approve' 
                        ? await handleApproveRestaurantRequest(requestId.toString(), accessToken, false)
                        : await handleRejectRestaurantRequest(requestId.toString(), accessToken, false);
                } else {
                    success = action === 'approve'
                        ? await handleApproveCourierRequest(requestId.toString(), accessToken, false)
                        : await handleRejectCourierRequest(requestId.toString(), accessToken, false);
                }

            }

            if (success) {
                WebApp.showAlert(`Заявка успешно ${action === 'approve' ? 'принята' : 'отклонена'}!`);
                await fetchRequests();
            } else {
                WebApp.showAlert(`Ошибка при ${action === 'approve' ? 'принятии' : 'отклонении'} заявки`);
            }
        } catch (err) {
            WebApp.showAlert("Произошла ошибка");
            console.error(`Ошибка при обработке заявки:`, err);
        } finally {
            setProcessingId(null);
        }
    };

    useEffect(() => {
        fetchRequests();
    }, []);

    return (<>
        <BackButton onClick={()=>navigate("/")}/>

        <div className="app_background_area">
            <div className="app_layout_area" style={isMobile ? { marginTop: '100px' } : {}}>
                <div className="request-page">
                    <div className="form-container">
                        {loading ? (
                            <div>Загрузка заявок...</div>
                        ) : error ? (
                            <div className="error-message">{error}</div>
                        ) : (
                            <div className="requests-list">
                                <h2>Заявки ресторанов</h2>
                                {requests?.restaurant_requests?.length ? (
                                    <ul className="requests-container">
                                        {requests.restaurant_requests.map(request => (
                                            <li key={request.request_id} className="request-item">
                                                <div className="request-info">
                                                    <h3>{request.restaurantName}</h3>
                                                    <p>Адрес: {request.address}</p>
                                                    <p>Телефон: {request.phone_number}</p>
                                                    <p>Описание: {request.description}</p>
                                                    <p>Клиент: {request.client_info.first_name} {request.client_info.last_name}</p>
                                                </div>
                                                <div className="request-actions">
                                                    <button 
                                                        onClick={() => handleRequestAction(request.request_id, true, 'approve')}
                                                        disabled={processingId === request.request_id}
                                                        className="approve-btn"
                                                    >
                                                        {processingId === request.request_id ? 'Обработка...' : 'Принять'}
                                                    </button>
                                                    <button 
                                                        onClick={() => handleRequestAction(request.request_id, true, 'reject')}
                                                        disabled={processingId === request.request_id}
                                                        className="reject-btn"
                                                    >
                                                        {processingId === request.request_id ? 'Обработка...' : 'Отклонить'}
                                                    </button>
                                                </div>
                                            </li>
                                        ))}
                                    </ul>
                                ) : (
                                    <p>Нет заявок ресторанов</p>
                                )}

                                <h2>Заявки курьеров</h2>
                                {requests?.courier_requests?.length ? (
                                    <ul className="requests-container">
                                        {requests.courier_requests.map(request => (
                                            <li key={request.request_id} className="request-item">
                                                <div className="request-info">
                                                    <h3>{request.client_info.first_name} {request.client_info.last_name}</h3>
                                                    <p>Номер машины: {request.car_number}</p>
                                                    <p>Описание: {request.request_description}</p>
                                                    <p>Телеграм: @{request.client_info.username}</p>
                                                </div>
                                                <div className="request-actions">
                                                    <button 
                                                        onClick={() => handleRequestAction(request.request_id, false, 'approve')}
                                                        disabled={processingId === request.request_id}
                                                        className="approve-btn"
                                                    >
                                                        {processingId === request.request_id ? 'Обработка...' : 'Принять'}
                                                    </button>
                                                    <button 
                                                        onClick={() => handleRequestAction(request.request_id, false, 'reject')}
                                                        disabled={processingId === request.request_id}
                                                        className="reject-btn"
                                                    >
                                                        {processingId === request.request_id ? 'Обработка...' : 'Отклонить'}
                                                    </button>
                                                </div>
                                            </li>
                                        ))}
                                    </ul>
                                ) : (
                                    <p>Нет заявок курьеров</p>
                                )}
                            </div>
                        )}
                    </div>
                </div>

                {isMobile && <div className="app_mobile_footer" style={{ zIndex: '15' }}>Симбир Еда</div>}
            </div>
        </div>
        </>);
};

export default RequestsPage;