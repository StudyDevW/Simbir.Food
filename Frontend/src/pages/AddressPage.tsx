import { use, useEffect, useState } from 'react'
import '../styles/AppStyle.sass'
import { YMaps, Map, Placemark, GeolocationControl } from '@pbe/react-yandex-maps';
import { useNavigate, useLocation } from 'react-router-dom';
import { handleUserAuth, handleUserRegister } from "../api-integrations/AuthAPI.ts";
import { vkUser, getUserData, initVKApp } from '../vk-integrations/InitData.ts';
import { BackButton } from '@twa-dev/sdk/react';
import { AuthComponent } from '../api-integrations/Interfaces/API_Interfaces.ts';

var YANDEX_API_KEY = import.meta.env.VITE_YANDEX_API_KEY;

let vkUserInstance: vkUser | null = null;

const MapsSearchBarAnimation: React.FC<{text: string}> = ({ text }) => {
    const [displayedText, setDisplayedText] = useState<string>('');
    const [index, setIndex] = useState<number>(0);
  
    useEffect(() => {
      if (index < text.length) {
        const timeout = setTimeout(() => {
          setDisplayedText((prev) => prev + text[index]);
          setIndex((prev) => prev + 1);
        }, 150); 
  
        return () => clearTimeout(timeout);
      }
    }, [index, text]);
  
    return <div className="app_maincontent_searchbar_text">{displayedText}</div>;
};

const AddressPage: React.FC = () => {

    const locationReact = useLocation();

    const address_default = locationReact.state?.address_default || '';

    const flag_state = locationReact.state?.flag || '';

    const navigate = useNavigate();
    //Yandex Integrations
    const [inputValue, setInputValue] = useState('');

    const [suggestions, setSuggestions] = useState<string[]>([]);

    const [loading, setLoading] = useState(false);
    //!Yandex Integrations

    const [isMobile, setIsMobile] = useState<boolean>(false);

    const [clickedMaps, setClickedMaps] = useState<boolean>(false);

    const [isAuthOperated, setIsAuthOperated] = useState<boolean>(false);

    const [keyboardFocused, setKeyboardFocused] = useState<boolean>(false);

    const [imageAvatar, setImageAvatar] = useState<string>("");

    const [address, setAddress] = useState<string | null>(null);

    const [location, setLocation] = useState<[number, number] | null>(null);

    const [locationNew, setLocationNew] = useState<[number, number] | null>(null);

    const [userData, setUserData] = useState<any>(null);

    const [initialized, setInitialized] = useState<boolean>(false);

    useEffect(() => {
        const init = async () => {
            try {
                const { user, isMobile: mobile } = await initVKApp();
                
                if (user) {
                    setUserData(user);
                    setImageAvatar(user.photo_200 || '');
                    
                    const platform = mobile ? 'Mobile' : 'PC';
                    vkUserInstance = new vkUser(user, mobile);
                    setIsMobile(mobile);
                }
                
                // // Настройка внешнего вида
                // await vkBridge.send('VKWebAppSetViewSettings', {
                //     status_bar_style: 'dark',
                //     action_bar_color: '#004681'
                // });
                
                if (address_default !== "") {
                    setInputValue(address_default);
                    setAddress(address_default);
                }
                
                setInitialized(true);
            } catch (error) {
                console.error('Ошибка инициализации VK:', error);
            }
        };
        
        init();
    }, []);

    useEffect(() => {
        const fetchSuggestions = async () => {
          if (inputValue.trim() === '') return;
    
          setLoading(true);
    
          try {
            const response = await fetch(`https://geocode-maps.yandex.ru/1.x/?apikey=${YANDEX_API_KEY}&geocode=Ульяновск, ${inputValue} 0&format=json`);
            const data = await response.json();
    
            if (data && data.response && data.response.GeoObjectCollection && data.response.GeoObjectCollection.featureMember) {
              const newSuggestions = data.response.GeoObjectCollection.featureMember.map((item: any) => item.GeoObject.name);
              
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
    }, [inputValue]);

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
    

    const onClickAddress = (address: string) => {
        setAddress(address || 'Адрес не найден');
    }

    useEffect(()=> {
        if (locationNew !== null) {
            setLocation(locationNew);
            setLocationNew(null);
        }
    }, [locationNew])

    useEffect(()=>{
        if (address !== null)
            getCoordinates(address);
    }, [address])

    useEffect(()=>{
        if (keyboardFocused) {

        }
    }, [keyboardFocused])

    const UserAuthRequestAPI = async (authvars: AuthComponent) => {

        const validate = await handleUserAuth(authvars);

        if (validate) {
            if (flag_state === "orderCreate")
                navigate("/ordered");
            else 
                navigate("/");
        }
    
    }

    useEffect(()=>{
        if (isAuthOperated) {
            if (address !== null)
                userData.SetAddress(address);

            UserAuthRequestAPI(userData.AuthData());
        }
    }, [isAuthOperated])

    const LoadingDraw = () => {
        return (<>
            <div className="app_loading_area" style={{}}>
                <div className="app_loading_letter">
                    <div className="app_loading_bar"></div>
                </div>
            </div>
        </>)
    }

    const handleMapClick = (event: any) => {
        const coordinates = event.get('coords');
        fetchSuggestion(coordinates[0], coordinates[1]);
    }

    return (
    <>

        {flag_state === "orderCreate" && <BackButton onClick={()=>navigate("/ordered")}/>}

        {flag_state !== "orderCreate" && <BackButton onClick={()=>navigate("/")}/>}

       

        <div className="app_background_area">

            <div className="app_layout_area" style={{}}>

                {!isAuthOperated && 
                <>
                    <div className="app_delivery_header" style={
                    {
                        backdropFilter: 'blur(8px)', 
                        position: 'fixed', 
                        background: 'linear-gradient(#EAEAEA, transparent)',
                        zIndex: '5',
                        maxWidth: '1000px'}
                    }>

                        <div className="app_delivery_header_image"></div>


                        {address !== null && 
                        <>
                            <div className="app_delivery_header_title_area">
                                <div className="app_delivery_header_title">{address}</div>
                                <div className="app_delivery_header_title big">Адрес доставки</div>
                            </div>
                        </>}

                        {address === null && 
                        <>
                            <div className="app_delivery_header_title_area">
                                <div className="app_delivery_header_title big" style={{
                                    marginTop: '15px'
                                }}>{`Введите адрес доставки`}</div>
                            </div>
                        </>
                        }

                

                        <div className="app_delivery_header_profile"
                        style={{
                            backgroundImage: `url(${imageAvatar})`,
                        }}></div>

                        
                    </div>


                    <YMaps>
                    

                        {location && <>
                            <Map 
                                state={{center: location || [54.314194, 48.419610], zoom: 17}} 
                                width="100%" height="100%"
                                onClick={handleMapClick}>
                                {<Placemark geometry={location} options={{
                                    // Options. You must specify this type of layout.
                                    iconLayout: 'default#image',
                                    // Custom image for the placemark icon.
                                    iconImageHref: "../../images/location.png",
                                    // The size of the placemark.
                                    iconImageSize: [40, 40],
                                    // The offset of the upper left corner of the icon relative
                                    // to its "tail" (the anchor point).
                                    iconImageOffset: [-18, -42]}}/>
                                }
                            </Map>
                        </>}

                        {location === null && <>
                            <Map 
                                defaultState={{ center: location || [54.314194, 48.419610], zoom: 10 }} 
                                width="100%" height="100%"
                                onClick={handleMapClick}>
                             
                            </Map>
                        </>}

                    </YMaps>

                    {(!keyboardFocused && address === null) && <>
                        <div className={"app_maincontent maps"} >
                            <div className="app_maincontent_title" 
                            style={{fontSize: "20px"}}>{"Куда доставлять заказы?"}</div>

                            <div className="app_maincontent_searchbar_decor" onClick={() => setKeyboardFocused(true)}>

                                <div className="app_maincontent_searchbar_icon"></div>

                                <MapsSearchBarAnimation text='Введите адрес'/>
                            </div>
                        </div>
                    </>} 

                    {(!keyboardFocused && address !== null) && <>
                        <div className={"app_maincontent maps"} style={
                            {
                                backdropFilter: 'blur(5px)', 
                                background: 'linear-gradient(transparent, #EAEAEA)',
                                borderRadius: '0px',
                                height: "60px"
                            }}>


                            <div className="app_maincontent_address_button_area">
                                <div className="app_maincontent_address_button_complete"
                                onClick={() => setIsAuthOperated(true)}>
                                    Все верно
                                </div>

                                <div className="app_maincontent_address_button_complete change"
                                onClick={() => setKeyboardFocused(true)}>
                                </div>
                            </div>

                    

                        </div>
                    </>} 

                    {keyboardFocused && 
                        <div className={"app_maincontent maps clicked"} >
                            <div className="app_maincontent_title" 
                            style={{fontSize: "20px"}}>{"Куда доставлять заказы?"}</div>

                            <div className="app_maincontent_searchbar_decor" style={{marginBottom: "20px"}}>
                                <input className='app_maincontent_searchbar'
                                    type="text"
                                    value={inputValue}
                                    onFocus={() => setKeyboardFocused(true)}
                                    onBlur={() => 
                                    { 
                                        if (inputValue === "")
                                            setKeyboardFocused(false);
                                    }}
                                    onChange={(e) => setInputValue(e.target.value)}
                                    placeholder={'Введите адрес'}
                                /> 

                                <div className="app_maincontent_searchbar_icon"></div>
                            </div>
                            
                            {suggestions.map((suggestion, index) => (
                            <div className="app_suggestion_button" key={index} onClick={() => {
                                onClickAddress(suggestion);
                                setKeyboardFocused(false);
                            }}>{suggestion}</div>
                            ))}
            
                        </div>
                    }


                </>}

                {(isAuthOperated && userData.GetAddress() !== "" && userData.GetDevice() !== "") && <>
                    {LoadingDraw()}
                </>}

                {/* {(isMobile) && <div className="app_mobile_footer" style={{zIndex: '15'}}>Симбир Еда</div>} */}

            </div>

        </div>
        
    </>)
}

export default AddressPage