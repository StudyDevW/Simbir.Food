import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import WebApp from "@twa-dev/sdk";
import { BackButton } from '@twa-dev/sdk/react';
import { handleOrdersForCourier } from '../../api-integrations/CourierAPI';
import { OrderForCourierDto } from "../../api-integrations/Interfaces/API_Interfaces";
import { StorageGetItem } from "../../telegram-integrations/cloudstorage/CloudStorage";
import '../../styles/AppStyle.sass'

const AvailableOrdersPage: React.FC = () => {
    const [isMobile, setIsMobile] = useState<boolean>(false);
    const [orders, setOrders] = useState<OrderForCourierDto[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    useEffect(() => {
        WebApp.setHeaderColor('#EAEAEA');
        WebApp.setBackgroundColor('#004681');
        
        if (WebApp.platform === 'ios' || WebApp.platform === 'android') {
            setIsMobile(true);
        }
        
        WebApp.ready();
        fetchOrders();
    }, []);

    const fetchOrders = async () => {
        setLoading(true);
        setError(null);
        
        try {
            const accessToken = await StorageGetItem("AccessToken");
            if (accessToken === "empty") {
                setError("Требуется авторизация");
                return;
            }
    
            const ordersData = await handleOrdersForCourier(accessToken);
            
            if (Array.isArray(ordersData)) {
                setOrders(ordersData);
                
                if (ordersData.length === 0) {
                    setError("Нет доступных заказов");
                }
            } else {
                setError("Не удалось загрузить данные");
            }
        } catch (err) {
            console.error("Fetch error:", err);
            setError(err instanceof Error ? err.message : "Ошибка при загрузке данных");
        } finally {
            setLoading(false);
        }
    };

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

    // const handleAcceptOrder = async (orderId: string) => {
    //     if (window.confirm("Вы уверены, что хотите принять этот заказ?")) {
    //         try {
    //             const accessToken = await StorageGetItem("AccessToken");
    //             if (accessToken === "empty") return;
                
    //             // Здесь должен быть вызов API для принятия заказа
    //             // await acceptOrderApiCall(orderId, accessToken);
    //             alert(`Заказ ${orderId} принят!`);
    //             fetchOrders(); // Обновляем список
    //         } catch (error) {
    //             console.error("Ошибка при принятии заказа:", error);
    //             alert("Не удалось принять заказ");
    //         }
    //     }
    // };

    return (
        <>
            <BackButton onClick={() => navigate(-1)} />
            <div className="app_background_area">
                <div className="app_layout_area" style={isMobile ? { marginTop: '100px' } : {}}>
                    <div className="scrollable-content">
                        <div className="orders-page">
                            <div className="order-page-font">Доступные заказы</div>
                            
                            {loading ? (
                                <div className="loading">Загрузка...</div>
                            ) : error ? (
                                <div className="error">{error}</div>
                            ) : (
                                <div className="orders-list">
                                    {orders.map(order => (
                                        <div key={order.orderId} className="order-card">
                                            <div className="order-header">
                                                <div className="order-header-font">Заказ #{order.orderId.substring(0, 8)}</div>
                                                <span style={{color: "white"}} className="order-date">
                                                    {formatDate(order.orderDate)}
                                                </span>
                                            </div>
                                            
                                            <div className="order-details">
                                                <div className="restaurant-info">
                                                    <p><strong>Ресторан:</strong> {order.restaurantName}</p>
                                                    <p><strong>Адрес ресторана:</strong> {order.restaurantAddress}</p>
                                                </div>
                                                
                                                <button 
                                                    className="accept-button"
                                                    onClick={() => navigate('/couriermap', {state: { orderInfo: order }})}
                                                >
                                                    Детали
                                                </button>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
};

export default AvailableOrdersPage;