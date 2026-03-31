import React, { useEffect, useState } from "react";
import { useLocation, useNavigate, useSearchParams } from "react-router-dom";
import WebApp from "@twa-dev/sdk";
import { BackButton } from '@twa-dev/sdk/react';
import { handleOrdersGetInRestaurant, handleAllOrdersGetForRestaurantOfAllTime } from '../../api-integrations/OrderAPI';
import { OrderInfo } from "../../api-integrations/Interfaces/API_Interfaces";
import { StorageGetItem } from "../../vk-integrations/cloudstorage/CloudStorage";
import { handleOrderMarkAsReadyByRestaurant, handleOrderRejectByRestaurant } from "../../api-integrations/RestaurantAPI";
import '../../styles/AppStyle.sass'

const RestaurantsOrdersOwnerPage: React.FC = () => {
    const [isMobile, setIsMobile] = useState<boolean>(false);
    const [orders, setOrders] = useState<OrderInfo[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [activeTab, setActiveTab] = useState<'current' | 'all'>('current');
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const restaurantId = searchParams.get('restaurantId');
    
    const locationReact = useLocation();
    
    const nameRestaurant = locationReact.state?.nameRest || '';

    useEffect(() => {
        WebApp.setHeaderColor('#EAEAEA');
        WebApp.setBackgroundColor('#004681');
        
        if (WebApp.platform === 'ios' || WebApp.platform === 'android') {
            setIsMobile(true);
        }
        
        WebApp.ready();
    }, []);

    const fetchOrders = async () => {
        setLoading(true);
        setError(null);
        
        try {
            if (!restaurantId) {
                setError("ID ресторана не указан");
                return;
            }
    
            const accessToken = await StorageGetItem("AccessToken");
            if (accessToken === "empty") {
                setError("Требуется авторизация");
                return;
            }
    
            let response;
            if (activeTab === 'current') {
                response = await handleOrdersGetInRestaurant(accessToken, restaurantId);
            } else {
                response = await handleAllOrdersGetForRestaurantOfAllTime(accessToken, restaurantId);
            }
            
            if (response) {
                setOrders(response);
                
                if (response.length === 0) {
                    setError(activeTab === 'current' 
                        ? "Нет текущих заказов" 
                        : "Нет заказов за всё время");
                }
            } else {
                setError("Не удалось загрузить данные");
            }
        } catch (err) {
            console.error("Fetch error:", err);
            setError(err instanceof Error ? err.message : "Неизвестная ошибка");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchOrders();
    }, [restaurantId]);
    
    const getStatusClass = (status: string) => {
        return status.toLowerCase().replace(/\s+/g, '-');
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

    const handleMarkAsReady = async (orderId: string) => {
        console.log(`Пометить заказ ${orderId} как готовый`);
        try {
            const accessToken = await StorageGetItem("AccessToken");
            if (accessToken === "empty") return;
            
            await handleOrderMarkAsReadyByRestaurant(accessToken, orderId);
            alert(`Статус заказа ${orderId} изменён на "Готов"`);
            fetchOrders();
        } catch (error) {
            console.error("Ошибка при изменении статуса:", error);
        }
    };
    
    const handleRejectOrder = async (orderId: string) => {
        if (!window.confirm("Вы уверены, что хотите отказаться от этого заказа?")) return;
        
        try {
            const accessToken = await StorageGetItem("AccessToken");
            if (accessToken === "empty") return;
            
            await handleOrderRejectByRestaurant(accessToken, orderId);
            alert(`Заказ ${orderId} отклонён`);
            fetchOrders();
        } catch (error) {
            console.error("Ошибка при отказе от заказа:", error);
        }
    };

    useEffect(() => {
        fetchOrders();
    }, [restaurantId, activeTab]);

    return (
        <>
            <BackButton onClick={() => navigate(-1)} />
            <div className="app_background_area">
                <div className="app_layout_area" style={isMobile ? { 
                    marginTop: '100px',
                    height: 'calc(100vh - 100px)'
                } : { height: '100vh' }}>




                    <div className="scrollable-content">
                        <div className="orders-page">
                            <div className="app_fooditem_header" style={{marginTop: `0px`,position: "relative", marginLeft: '0px', width: '100%', marginBottom: '15px'}}>
                                <div className="app_fooditem_header_title" style={{marginBottom: '0px'}}>{`${nameRestaurant}`}</div>

                                <div className="app_fooditem_header_title" style={{marginTop: '5px', alignContent: "flex-start", fontSize: '15px'}}>{`Заказы ресторана`}</div>
                            </div>
                            
                            <div className="orders-tabs">
                                <button 
                                    className={`tab-button ${activeTab === 'current' ? 'active' : ''}`}
                                    onClick={() => setActiveTab('current')}
                                >
                                    Текущие заказы
                                </button>
                                <button 
                                    className={`tab-button ${activeTab === 'all' ? 'active' : ''}`}
                                    onClick={() => setActiveTab('all')}
                                >
                                    Все заказы
                                </button>
                            </div>
                            
                            {loading ? (
                                <div className="loading">Загрузка...</div>
                            ) : error ? (
                                <div className="error">{error}</div>
                            ) : (
                                <div className="orders-list">
                                    {orders.map(order => (
                                        <div key={order.order_id} className="order-card">
                                            <div className="order-header">
                                                <div className="order-header-font">Заказ #{order.order_id.substring(0, 8)}</div>
                                                <span className={`status ${getStatusClass(order.status_order)}`} style={{color: "white"}}>
                                                    {order.status_order}
                                                </span>
                                            </div>
                                            
                                            <div className="order-details">
                                                <div className="order-info">
                                                    <p><strong>Дата заказа:</strong> {formatDate(order.order_date)}</p>
                                                    <p><strong>Сумма:</strong> {order.price_order} ₽</p>
                                                    <p><strong>Адрес доставки:</strong> {order.client_address}</p>
                                                </div>
                                                
                                                <div className="food-items">
                                                    <h4>Состав заказа:</h4>
                                                    <ul>
                                                        {order.food_items.map((item, index) => (
                                                            <li key={index} className="food-item">
                                                                <div className="food-item-image">
                                                                    <img src={item.image || '/default-food.png'} alt={item.name} />
                                                                </div>
                                                                <div className="food-item-info">
                                                                    <p>{item.name}</p>
                                                                    <p>{item.price} ₽ · {item.weight}g · {item.calories} kcal</p>
                                                                </div>
                                                            </li>
                                                        ))}
                                                    </ul>
                                                </div>
    
                                                {activeTab === 'current' && (
                                                    <div className="order-actions">
                                                        <button 
                                                            className="action-button ready-button"
                                                            onClick={() => handleMarkAsReady(order.order_id)}
                                                        >
                                                            Изменить статус на "Готов"
                                                        </button>
                                                        <button 
                                                            className="action-button reject-button"
                                                            onClick={() => handleRejectOrder(order.order_id)}
                                                        >
                                                            Отказаться от заказа
                                                        </button>
                                                    </div>
                                                )}
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

export default RestaurantsOrdersOwnerPage;