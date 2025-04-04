import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import WebApp from "@twa-dev/sdk";
import { BackButton } from '@twa-dev/sdk/react';
import { handleRestaurantsForUser } from '../../api-integrations/ClientInfoAPI';
import { RestaurantsInfoForOwner } from "../../api-integrations/Interfaces/API_Interfaces";
import { StorageGetItem } from "../../telegram-integrations/cloudstorage/CloudStorage";
import '../../styles/AppStyle.sass'

const STATUSES = {
    1: { text: 'Активен', className: 'status-active' },
    0: { text: 'Неактивен', className: 'status-inactive' },
    2: { text: 'Заморожен', className: 'status-frozen' }
};

const RestaurantsOwnerPage: React.FC = () => {
    const [isMobile, setIsMobile] = useState<boolean>(false);
    const [restaurants, setRestaurants] = useState<RestaurantsInfoForOwner[]>([]);
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
        fetchRestaurants();
    }, []);

    const getSafeImageUrl = (imgPath: string) => {
        const invalidPaths = ['нету', 'нет', 'undefined', 'null', ''];
        return imgPath && !invalidPaths.some(bad => 
            imgPath.toLowerCase().includes(bad)
        ) ? imgPath : '/default-restaurant.jpg';
    };

    const fetchRestaurants = async () => {
        setLoading(true);
        setError(null);
        
        try {
            const accessToken = await StorageGetItem("AccessToken");
            if (accessToken === "empty") {
                setError("Требуется авторизация");
                return;
            }
    
            const restaurantsData = await handleRestaurantsForUser(accessToken);
            
            if (Array.isArray(restaurantsData)) {
                setRestaurants(restaurantsData);
                
                if (restaurantsData.length === 0) {
                    setError("Вы не являетесь владельцем ресторанов.");
                }
            } else {
                setError("Не удалось загрузить данные");
            }
        } catch (err) {
            console.error("Fetch error:", err);
            setError("Ошибка при загрузке данных");
        } finally {
            setLoading(false);
        }
    };

    const getStatusInfo = (status: number) => {
        return STATUSES[status as keyof typeof STATUSES] || STATUSES[0];
    };

    const handleRestaurantClick = (restaurantId: string) => {
        navigate(`/restaurantsOwner-orders?restaurantId=${restaurantId}`)
    };

    return (
        <>
            <BackButton onClick={() => navigate(-1)} />
            <div className="app_background_area">
                <div className="app_layout_area" style={isMobile ? { marginTop: '100px' } : {}}>
                    <div className="scrollable-content">
                        <div className="restaurantOwner-page">
                            <h2>Список ресторанов</h2>
                            
                            {loading ? (
                                <div className="loading">Загрузка...</div>
                            ) : error ? (
                                <div className="error">{error}</div>
                            ) : (
                                <div className="restaurantsOwner-list">
                                    {restaurants.map(restaurant => {
                                        const statusInfo = getStatusInfo(restaurant.status);
                                        return (
                                            <div 
                                                key={restaurant.id} 
                                                className="restaurantOwner-card"
                                                onClick={() => handleRestaurantClick(restaurant.id)}
                                            >
                                                <img 
                                                    src={getSafeImageUrl(restaurant.imagePath)}
                                                    alt={restaurant.restaurantName}
                                                    className="restaurantOwner-image"
                                                    loading="lazy"
                                                    onError={(e) => {
                                                        (e.target as HTMLImageElement).src = '/default-restaurant.jpg';
                                                    }}
                                                />
                                                <div className="restaurantOwner-info">
                                                    <h3>{restaurant.restaurantName}</h3>
                                                    <p>Адрес: {restaurant.address}</p>
                                                    <p className={`status ${statusInfo.className}`}>
                                                        Статус: {statusInfo.text}
                                                    </p>
                                                </div>
                                            </div>
                                        );
                                    })}
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
};

export default RestaurantsOwnerPage;