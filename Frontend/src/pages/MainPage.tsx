import { useEffect, useState } from 'react'
import '../styles/AppStyle.sass'
import WebApp from '@twa-dev/sdk';
import { StorageGetItem, StorageSetItem, StorageDeleteItem } from '../telegram-integrations/cloudstorage/CloudStorage.ts';
import { useNavigate, useLocation, data } from 'react-router-dom';
import { handleGetInfoMe } from '../api-integrations/ClientInfoAPI.ts';
import { handleUserAuth } from '../api-integrations/AuthAPI.ts';
import { telegramUser } from '../telegram-integrations/InitData.ts';
import ProfilePage from './ProfilePage.tsx';
import { AuthComponent, GetMeInfo } from '../api-integrations/Interfaces/API_Interfaces.ts';

var userData = new telegramUser(
  WebApp.initDataUnsafe.user, 
  (WebApp.platform === 'ios' || WebApp.platform === 'android')
);

const MainPage: React.FC = () => {

  const navigate = useNavigate();

  const [isMobile, setIsMobile] = useState<boolean>(false);
  
  const [userInfo, setUserInfo] = useState<GetMeInfo | null>(null);

  const [logined, setLogined] = useState<boolean>(false);

  const [profileOpened, setProfileOpened] = useState<boolean>(false);

  const GetUserRequestAPI = async (accessToken: string) => {
    
    const getuser = await handleGetInfoMe(accessToken);
    
    if (getuser !== null) {
      setUserInfo(getuser);
    }
  }

  const UserAuthRequestAPI = async (authvars: AuthComponent) => {
    const validate = await handleUserAuth(authvars);

    if (validate) {
      setLogined(true);
    }
    else {
      navigate("/onboarding");
    }
  }

  const ProfileGet = async () => {
    const accessToken: string = await StorageGetItem('AccessToken');

    if (accessToken !== "empty") {
      await GetUserRequestAPI(accessToken);
    }
  }


  useEffect(() => {
    WebApp.setHeaderColor('#EAEAEA');

    WebApp.setBackgroundColor('#004681');

    if (WebApp.platform === 'ios' || WebApp.platform === 'android')
      setIsMobile(true);
    else 
      setIsMobile(false);

    WebApp.ready();

    userData.SetAddress("NO_CHANGE");

    UserAuthRequestAPI(userData.AuthData());
    
  }, []);

  useEffect(() => {
    if (logined) {
      ProfileGet();
    }
  }, [logined])

  useEffect(()=>{
    if (isMobile) {
        WebApp.lockOrientation();
        WebApp.requestFullscreen();
    }
  }, [isMobile])

  const LoadingDraw = () => {
    return (<>
        <div className="app_loading_area" style={ isMobile ? { height: 'calc(100% - 100px - 45px)' } : {} }>
            <div className="app_loading_letter">
                <div className="app_loading_bar"></div>
            </div>
        </div>
    </>)
  }
 
  return (
    <>
        <div className="app_background_area">

            <div className="app_layout_area" style={ isMobile ? { marginTop: '100px' } : {}}>

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
                  </div>
                </>}
             

                {(isMobile) && <div className="app_mobile_footer">Симбир Еда</div>}

            </div>

        </div>
    </>
  )
}

export default MainPage
