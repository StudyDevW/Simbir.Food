import React, { useEffect, useState, useRef } from "react";
import { useNavigate } from "react-router-dom";
import WebApp from "@twa-dev/sdk";
import { StorageGetItem } from "../../telegram-integrations/cloudstorage/CloudStorage";
import { handleRestaurantRequest } from "../../api-integrations/RequestAPI";
import { YMaps, Map, Placemark } from '@pbe/react-yandex-maps';
import { BackButton } from '@twa-dev/sdk/react';

var YANDEX_API_KEY = import.meta.env.VITE_YANDEX_API_KEY;

const RestaurantRequestPage: React.FC = () => {
    const [restaurantName, setRestaurantName] = useState<string>("");
    const [phoneNumber, setPhoneNumber] = useState<string>("");
    const [description, setDescription] = useState<string>("");
    const [imagePath, setImagePath] = useState<string>("");
    const [openTime, setOpenTime] = useState<string>("");
    const [closeTime, setCloseTime] = useState<string>("");
    const [requestDescription, setRequestDescription] = useState<string>("");
    const [isMobile, setIsMobile] = useState<boolean>(false);
    
    const [address, setAddress] = useState<string>("");
    const [inputValue, setInputValue] = useState('');
    const [location, setLocation] = useState<[number, number] | null>(null);
    const [locationNew, setLocationNew] = useState<[number, number] | null>(null);
    const [suggestions, setSuggestions] = useState([]);
    const [isUserTyping, setIsUserTyping] = useState(true);
    const [loading, setLoading] = useState(false);

    const navigate = useNavigate();

    useEffect(()=>{
                WebApp.setHeaderColor('#EAEAEA');
        
                WebApp.setBackgroundColor('#004681');
            
                if (WebApp.platform === 'ios' || WebApp.platform === 'android')
                  setIsMobile(true);
                else 
                  setIsMobile(false);
            
                WebApp.ready();
            }, [WebApp]);

    useEffect(() => {
        const fetchSuggestions = async () => {
            if (!isUserTyping || inputValue.trim() === '') {
            setSuggestions([]);
            return;
            }
            
            setLoading(true);
            try {
            const response = await fetch(`https://geocode-maps.yandex.ru/1.x/?apikey=${YANDEX_API_KEY}&geocode=Ульяновск,${inputValue} &kind=house&format=json`);
            const data = await response.json();
            
            if (data?.response?.GeoObjectCollection?.featureMember) {
                const newSuggestions = data.response.GeoObjectCollection.featureMember.map((item: any) => {
                return item.GeoObject.name;
                });
                setSuggestions(newSuggestions);
            }
            } catch (error) {
            console.error('Ошибка при получении данных:', error);
            } finally {
            setLoading(false);
            }
        };
        
        const debounceFetch = setTimeout(fetchSuggestions, 300);
        return () => clearTimeout(debounceFetch);
        }, [inputValue, isUserTyping]);

    useEffect(()=>{
        if (address !== null)
            getCoordinates(address);
    }, [address])

    useEffect(()=> {
        if (locationNew !== null) {
            setLocation(locationNew);
            setLocationNew(null);
        }
    }, [locationNew])
    
    const getCoordinates = async (address: string) => {
        try {
          const response = await fetch(`https://geocode-maps.yandex.ru/1.x/?apikey=${YANDEX_API_KEY}&geocode=${encodeURIComponent(`Ульяновск, ${address}`)}&format=json`);
          const data = await response.json();
    
          if (data.response.GeoObjectCollection.featureMember.length > 0) {
            const [lon, lat] = data.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos.split(' ').map(Number);
     
            setLocationNew([lat, lon]);
          } else {
            alert('Адрес не найден');
          }
        } catch (error) {
            alert('Ошибка при получении координат');
        }
    };

    const fetchSuggestion = async (latitude: number, longitude: number) => {

        try {
            const response = await fetch(`https://geocode-maps.yandex.ru/1.x/?apikey=${YANDEX_API_KEY}&geocode=${longitude},${latitude}&format=json`);
            const data = await response.json();
        
            if (data && data.response && data.response.GeoObjectCollection && data.response.GeoObjectCollection.featureMember) {
                const newSuggestion = data.response.GeoObjectCollection.featureMember[0].GeoObject.name;
                setInputValue(newSuggestion);
                setAddress(newSuggestion);
            }
        } catch (error) {
            console.error('Ошибка при получении данных:', error);
        } 
    };
    
    const handleMapClick = (event: any) => {
        const coordinates = event.get('coords');
        setIsUserTyping(false);
        fetchSuggestion(coordinates[0], coordinates[1]);
    };

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
        
    return (<>
        <BackButton onClick={()=>navigate("/")}/>
        <div className="app_background_area">
        
            <div className="app_layout_area" style={ isMobile ? { marginTop: '100px' } : {}}>

            <div className="request-page">
                <h2>Регистрация ресторана</h2>
                <div className="map-wrapper">
                    <YMaps>
                        <div className="map-search-container">
                        <input
                            type="text"
                            className="map-search-input"
                            value={inputValue}
                            onChange={(e) => {
                                setInputValue(e.target.value);
                                setIsUserTyping(true);
                              }}
                            placeholder="Поиск адреса..."
                        />
                        {suggestions.length > 0 && (
                            <div className="map-suggestions">
                            {suggestions.map((suggestion, index) => (
                                <div
                                key={index}
                                className="map-suggestion-item"
                                onClick={() => {
                                    setAddress(suggestion);
                                    setInputValue(suggestion);
                                    setSuggestions([]);
                                    setIsUserTyping(false);
                                  }}
                                >
                                {suggestion}
                                </div>
                            ))}
                            </div>
                        )}
                        </div>

                        <Map
                        key={location ? location.join(",") : "default"}
                        state={{ center: location || [54.314194, 48.419610], zoom: 17 }}
                        width="100%"
                        height="300px"
                        onClick={handleMapClick}
                        className="map-container"
                        >
                        {location && (
                            <Placemark
                            geometry={location}
                            options={{
                                iconLayout: "default#image",
                                iconImageHref: "../../images/location.png",
                                iconImageSize: [40, 40],
                                iconImageOffset: [-18, -42],
                            }}
                            />
                        )}
                        </Map>
                    </YMaps>
                </div>
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
                        readOnly
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

    </>);
};

export default RestaurantRequestPage;
