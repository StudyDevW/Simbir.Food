import { useEffect, useState } from 'react';
import '../styles/AppStyle.sass';
//import WebApp from '@twa-dev/sdk';
import { StorageGetItem, StorageSetItem, StorageDeleteItem } from '../vk-integrations/cloudstorage/CloudStorage.ts';
import { useNavigate, useLocation, data } from 'react-router-dom';
import { handleGetInfoMe } from '../api-integrations/ClientInfoAPI.ts';
import { handleUserAuth } from '../api-integrations/AuthAPI.ts';
import { vkUser, initVKApp } from '../vk-integrations/InitData.ts';
import ProfilePage from './ProfilePage.tsx';
import { AuthComponent, GetMeInfo, RestaurantInfo } from '../api-integrations/Interfaces/API_Interfaces.ts';
import { handleRestaurantsInfo, handleRestaurantsInfoWithSearch } from '../api-integrations/RestaurantAPI.ts';
import { handleLoadImage } from '../api-integrations/ImageAPI.ts';
import { loadingComponent } from '../LoadingComponent.ts';

// var userData = new telegramUser(
//   WebApp.initDataUnsafe.user, 
//   (WebApp.platform === 'ios' || WebApp.platform === 'android')
// );

import vkBridge from '@vkontakte/vk-bridge';

let vkUserInstance: vkUser | null = null;

var loadingInformation = new loadingComponent();

const RestaurantItemComponent: React.FC<{info: RestaurantInfo, isMobile: boolean, onClick: () => void}> = ({info, isMobile, onClick}) => {
  
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

  return (<>
    {(!runningA || loadingInformation.getLoading()) && <>
      <div className="app_maincontent_restaurant_block loading" style={{}}>
        <div className="app_maincontent_restaurant_block_image loading"></div>

        <div className="app_maincontent_restaurant_block_title loading"></div>

      </div>
    </>}

    {(runningA && !loadingInformation.getLoading()) && <>
      <div className="app_maincontent_restaurant_block" onClick={onClick} style={{}}>
        <div className="app_maincontent_restaurant_block_image" style={{
          backgroundImage: `url(${imageRendered})`
        }}></div>

        <div className="app_maincontent_restaurant_block_title">{`${info.restaurantName}`}</div>

        <div className="app_maincontent_restaurant_block_subtitle_area">
          <div className="app_maincontent_restaurant_block_subtitle_mark">{`${info.average_mark}`}</div>
          <div className="app_maincontent_restaurant_block_subtitle_markimg"></div>

          {isTimeInRange(info.open_time, info.close_time) && <>
            <div className="app_maincontent_restaurant_block_subtitle_opened">{'Открыто'}</div>
          </>}

          {!isTimeInRange(info.open_time, info.close_time) && <>
            <div className="app_maincontent_restaurant_block_subtitle_closed">{'Закрыто'}</div>
            <div className="app_maincontent_restaurant_block_subtitle_closed info">{`Открытие в ${info.open_time}`}</div>
          </>}

        </div>
      </div>
    </>}

  </>)
}

const MainPage: React.FC = () => {

  const navigate = useNavigate();

  const [isMobile, setIsMobile] = useState<boolean>(false);
  
  const [userInfo, setUserInfo] = useState<GetMeInfo | null>(null);

  const [logined, setLogined] = useState<boolean>(false);

  const [profileOpened, setProfileOpened] = useState<boolean>(false);

  const [restaurants, setRestaurantsInfo] = useState<RestaurantInfo[] | null>(null);

  const [inputValue, setInputValue] = useState('');

  const [keyboardFocused, setKeyboardFocused] = useState<boolean>(false);

  const [initialized, setInitialized] = useState<boolean>(false);


  const GetUserRequestAPI = async (accessToken: string) => {
    
    const getuser = await handleGetInfoMe(accessToken);
    
    if (getuser !== null) {
      setUserInfo(getuser);
    }
  }

  const GetRestaurantsRequestAPI = async (accessToken: string) => {

    const restaurantsinfo = await handleRestaurantsInfo(accessToken);

    if (restaurantsinfo !== null && inputValue === '') {
      setRestaurantsInfo(restaurantsinfo);
    }

  }

  const GetRestaurantsWithSearchRequestAPI = async (accessToken: string) => {

    const restaurantsinfo = await handleRestaurantsInfoWithSearch(accessToken, inputValue);

    if (restaurantsinfo !== null) {
      setRestaurantsInfo(restaurantsinfo);
    }

  }

  const UserAuthRequestAPI = async (authvars: AuthComponent) => {
    const validate = await handleUserAuth(authvars);
    if (validate) {
      setLogined(true);
    } else {
      navigate("/onboarding");
    }
  }

  const ProfileGet = async () => {
    const accessToken: string = await StorageGetItem('AccessToken');

    if (accessToken !== "empty") {
      await GetUserRequestAPI(accessToken);
    }
  }

  const RestaurantsGet = async () => {
    const accessToken: string = await StorageGetItem('AccessToken');

    if (accessToken !== "empty") {
      await GetRestaurantsRequestAPI(accessToken);
    }
  }

  const RestaurantsGetSearched = async () => {
    const accessToken: string = await StorageGetItem('AccessToken');

    if (accessToken !== "empty") {
      await GetRestaurantsWithSearchRequestAPI(accessToken);
    }
  }

  useEffect(() => {
    const init = async () => {
      try {
        const { user, isMobile: mobile } = await initVKApp();
        setIsMobile(mobile);
        
        if (user) {
          vkUserInstance = new vkUser(user, mobile);
          vkUserInstance.SetAddress("NO_CHANGE");
          await UserAuthRequestAPI(vkUserInstance.AuthData());
        }
        
        setInitialized(true);
      } catch (error) {
        console.error('Ошибка инициализации VK:', error);
        // Если ошибка, отправляем на онбординг
        navigate("/onboarding");
      }
    };
    
    init();
  }, []);

  useEffect(()=> {
    if (logined) {
      if (inputValue !== '') {
        setRestaurantsInfo(null);
        RestaurantsGetSearched();
      }
      else {
        setRestaurantsInfo(null);
        RestaurantsGet();
      }
    }
  }, [inputValue])

  useEffect(() => {
    if (logined) {
      ProfileGet();
      RestaurantsGet();
    }
  }, [logined])

  useEffect(()=>{
    if (isMobile) {
        // WebApp.lockOrientation();
        // WebApp.requestFullscreen();
    }
  }, [isMobile])

  const LoadingDraw = () => {
    return (<>
        <div className="app_loading_area" style={ {} }>
            <div className="app_loading_letter">
                <div className="app_loading_bar"></div>
            </div>
        </div>
    </>)
  }
 
  return (
    <>
        <div className="app_background_area">

            <div className="app_layout_area" style={{}}>

                {(profileOpened && userInfo !== null) && 
                  <ProfilePage isMobile={isMobile} info={userInfo} onChange={setProfileOpened} />
                }

                {userInfo !== null && <>
                  <div className="app_delivery_header">
                    <div className="app_delivery_header_image"></div>

                    {userInfo.address !== null && 
                        <>
                            <div className="app_delivery_header_title_area" onClick={()=>navigate("/address_select", { state: { address_default: userInfo.address } })}>
                                <div className="app_delivery_header_title">{userInfo.address}</div>
                                <div className="app_delivery_header_title big">Адрес доставки</div>
                            </div>
                    </>}

                    {userInfo.address === null && 
                    <>
                        <div className="app_delivery_header_title_area" onClick={()=>navigate("/address_select")}>
                            <div className="app_delivery_header_title big" style={{
                                marginTop: '15px'
                            }}>{`Укажите адрес доставки`}</div>
                        </div>
                    </>
                    }

                    <div className="app_delivery_header_profile"
                    onClick={()=>setProfileOpened(true)}
                    style={{
                            backgroundImage: `url(${userInfo.photo_url})`,
                    }}></div>
                  </div>
                </>}

               


                {userInfo === null && LoadingDraw()}

                {userInfo !== null && <>

                  <div className="app_maincontent">
                    <div className="app_maincontent_title">Рестораны</div>

                    <div className="app_maincontent_searchbar_decor restaurants" style={{marginBottom: "20px"}}>
                      <input className='app_maincontent_searchbar'
                          type="text"
                          value={inputValue}
                          onChange={(e) => setInputValue(e.target.value)}
                          placeholder={'Поиск'}
                          style={{backgroundColor: '#EAEAEA'}}
                      /> 

                      <div className="app_maincontent_searchbar_icon"></div>
                    </div>

                    <div className="app_maincontent_restaurant_block_area" style={{}}>



                      {restaurants !== null && restaurants.map((restaurant, index) => <>

                        <RestaurantItemComponent 
                          key={index} 
                          info={restaurant} 
                          isMobile={isMobile} 
                          onClick={() => navigate("/fooditems",  
                            { state: { restaurantInfo: restaurant } }
                          )}/>

                      </>)}

                    </div>

                  </div>
                </>}
             

                {/* {(isMobile) && <div className="app_mobile_footer">Симбир Еда</div>} */}

            </div>

        </div>
    </>
  )
}

export default MainPage
