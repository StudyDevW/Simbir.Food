import React, { use, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import WebApp from "@twa-dev/sdk";
import { BackButton } from '@twa-dev/sdk/react';
import { handleRestaurantsForUser } from '../../api-integrations/ClientInfoAPI';
import { RestaurantInfo, RestaurantsInfoForOwner } from "../../api-integrations/Interfaces/API_Interfaces";
import { StorageGetItem } from "../../vk-integrations/cloudstorage/CloudStorage";
import '../../styles/AppStyle.sass'
import { handleLoadImage } from "../../api-integrations/ImageAPI";
import { loadingComponent } from "../../LoadingComponent";

const STATUSES = {
    1: { text: 'Активен', className: 'status-active' },
    0: { text: 'Неактивен', className: 'status-inactive' },
    2: { text: 'Заморожен', className: 'status-frozen' }
};

var loadingInformation = new loadingComponent();

const RestaurantItemComponent: React.FC<{info: RestaurantsInfoForOwner, isMobile: boolean, onClick: () => void}> = ({info, isMobile, onClick}) => {
  
    //TODO: сделать рендер изображения
    const [imageRendered, setImageRendered] = useState<string | null>(null);
    
    const [runningA, setRunningA] = useState<boolean>(false);
  
    const renderImage = async () => {
  
      const accessToken: string = await StorageGetItem('AccessToken');
  
      if (accessToken !== "empty") {
        const imageItem = await handleLoadImage(accessToken, info.imagePath);
  
        if (imageItem !== null)
            setImageRendered(imageItem);
      }
    }
  
    useEffect(() => {
      loadingInformation.startLoading();
      loadingInformation.startLoadingAnimation();
  
      renderImage();
    }, [])
  
  
    const AnimationChecker = () => {
      if (loadingInformation.getStatusLoadingAnimation().isCompleted) {
          setRunningA(true);
      }
    }
  
    useEffect(()=>{
        if (!runningA) { 
            const intervalId = setInterval(()=>AnimationChecker(), 2000); 
  
            return () => clearInterval(intervalId);    
        }
    }, [runningA])
  
  
    useEffect(()=> {
      if (imageRendered !== null) {
        loadingInformation.endLoading();
      }
    }, [imageRendered])
  
    const isTimeInRange = (startTime: string, endTime: string): boolean => {
      const currentTime = new Date();
      const start = new Date();
      const end = new Date();
      
      const [startHours, startMinutes] = startTime.split(':').map(Number);
      const [endHours, endMinutes] = endTime.split(':').map(Number);
  
      start.setHours(startHours, startMinutes, 0, 0);
      end.setHours(endHours, endMinutes, 0, 0);
      
      return currentTime >= start && currentTime <= end;
    }
  
    const getStatusInfo = (status: number) => {
        return STATUSES[status as keyof typeof STATUSES] || STATUSES[0];
    };

    const statusInfo = getStatusInfo(info.status);

    return (<>
      {(!runningA || loadingInformation.getLoading()) && <>
        <div className="app_maincontent_restaurant_block loading" style={isMobile ? {height: '250px'} : {}}>
          <div className="app_maincontent_restaurant_block_image loading"></div>
  
          <div className="app_maincontent_restaurant_block_title loading"></div>
  
        </div>
      </>}
  
      {(runningA && !loadingInformation.getLoading()) && <>
        <div className="app_maincontent_restaurant_block" onClick={onClick} style={isMobile ? {height: '250px'} : {}}>
          <div className="app_maincontent_restaurant_block_image" style={{
            backgroundImage: `url(${imageRendered})`
          }}></div>
  
          <div className="app_maincontent_restaurant_block_title">{`${info.restaurantName}`}</div>
  
          <div className="app_maincontent_restaurant_block_subtitle_area">
            
            <div className="app_maincontent_restaurant_block_subtitle_mark">{`${info.address}`}</div>
                
            <div className={`app_maincontent_restaurant_block_subtitle_status ${statusInfo.className}`}>{"Статус: " + statusInfo.text}</div>
          </div>
    
        </div>
      </>}
  
    </>)
}

// const RestaurantOwnItem: React.FC<{restaurant: RestaurantsInfoForOwner}> = ({restaurant}) => {

//     const navigate = useNavigate();

//     const [imageRendered, setImageRendered] = useState<string | null>(null);


//     const getStatusInfo = (status: number) => {
//         return STATUSES[status as keyof typeof STATUSES] || STATUSES[0];
//     };

//     const statusInfo = getStatusInfo(restaurant.status);

//     const handleRestaurantClick = (restaurantId: string) => {
//         navigate(`/restaurantsOwner-orders?restaurantId=${restaurantId}`, {state: {nameRest: restaurant.restaurantName}})
//     };

//     const renderImage = async () => {

//         const accessToken: string = await StorageGetItem('AccessToken');
    
//         if (accessToken !== "empty") {
//           const imageItem = await handleLoadImage(accessToken, restaurant.imagePath);
    
//           if (imageItem !== null)
//               setImageRendered(imageItem);
//         }
//     }

//     useEffect(() => {
//       renderImage();
//     }, [])

//     return (
//         <div 
//             key={restaurant.id} 
//             className="restaurantOwner-card"
//             onClick={() => handleRestaurantClick(restaurant.id)}
//         >
//             <div className="restaurantOwner-image" style={{
//                 backgroundImage: `url(${imageRendered})`,
//                 backgroundPosition: 'center',
//                 backgroundRepeat: 'no-repeat',
//                 backgroundSize: 'cover'
//             }}></div>

//             {/* <img 
//                 src={getSafeImageUrl()}
//                 alt={restaurant.restaurantName}
//                 className="restaurantOwner-image"
//                 loading="lazy"
//                 onError={(e) => {
//                     (e.target as HTMLImageElement).src = '/default-restaurant.jpg';
//                 }}
//             /> */}
//             <div className="restaurantOwner-info">
//                 <h3>{restaurant.restaurantName}</h3>
//                 <p>Адрес: {restaurant.address}</p>
//                 <p className={`status ${statusInfo.className}`}>
//                     Статус: {statusInfo.text}
//                 </p>
//             </div>
//         </div>
//     );
// }

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





    return (
        <>
            <BackButton onClick={() => navigate(-1)} />
            <div className="app_background_area">
                <div className="app_layout_area" style={isMobile ? { marginTop: '100px' } : {}}>
                    {/* <div className="scrollable-content">
                        <div className="restaurantOwner-page">
                            <h2>Список ресторанов</h2>
                            
                            {loading ? (
                                <div className="loading">Загрузка...</div>
                            ) : error ? (
                                <div className="error">{error}</div>
                            ) : (
                                <div className="restaurantsOwner-list">
                                    {restaurants.map((restaurant, index) => <>
                                        <RestaurantOwnItem key={index} restaurant={restaurant}/>
                                    </>)}
                                </div>
                            )}
                        </div>
                    </div> */}

                    <div className="app_maincontent" style={{height: '100%'}}>
                        <div className="app_maincontent_title" style={{fontSize: '20px'}}>Рестораны во владении</div>

                        <div className="app_maincontent_restaurant_block_area" style={isMobile ? {height: 'calc(100% - 200px)', marginTop: '10px'} : {marginTop: '60px', height: 'calc(100% - 60px)'}}>

                        {restaurants !== null && restaurants.map((restaurant, index) => <>

                            <RestaurantItemComponent 
                                key={index} 
                                info={restaurant} 
                                isMobile={isMobile} 
                                onClick={() => navigate(`/restaurantsOwner-orders?restaurantId=${restaurant.id}`, {state: {nameRest: restaurant.restaurantName}})}/>

                        </>)}

                        </div>

                    </div>
                </div>
            </div>
        </>
    );
};

export default RestaurantsOwnerPage;