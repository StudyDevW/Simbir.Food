import { useEffect, useState } from 'react'
import '../styles/AppStyle.sass'
import WebApp from '@twa-dev/sdk';
import { StorageGetItemAsync, StorageSetItem, StorageDeleteItem } from '../cloudstorage-telegram/CloudStorage.ts';
import { useNavigate, useLocation } from 'react-router-dom';
import { handleGetInfoMe } from '../api-integrations/ClientInfoAPI.ts';
import { handleUserAuth } from '../api-integrations/AuthAPI.ts';


interface GetMeInfo {
  Id: string,
  telegram_id: number,
  first_name: string,
  last_name: string | null,
  username: string | null,
  photo_url: string | null,
  chat_id: number,
  address: string | null,
  roles: string[]
}

interface AuthComponent {
  id: number,
  first_name: string,
  last_name: string | null,
  username: string | null,
  is_bot: boolean,
  photo_url: string | null,
  chat_id: number,
  address: string,
  device: string,
  roles: string[]
}

let tg_component: AuthComponent;

const MainPage: React.FC = () => {

  const navigate = useNavigate();

  const [isMobile, setIsMobile] = useState<boolean>(false);
  
  const [userInfo, setUserInfo] = useState<GetMeInfo | null>(null);

  const [logined, setLogined] = useState<boolean>();

  const GetUserRequestAPI = async (accessToken: string) => {
    
    var getuser = await handleGetInfoMe(accessToken);
    
    if (getuser !== null) {
      setUserInfo(getuser);
    }
    else {
      const register_callback: string | undefined = await StorageGetItemAsync("RegisterCallback");

      if (register_callback !== undefined && register_callback === "register_requested") {
        TokenGetRequestAuth();
        
        await UserAuthRequestAPI(tg_component);
      }
    }
  }

  const UserAuthRequestAPI = async (authvars: AuthComponent) => {
    var validate = await handleUserAuth(authvars);

    if (validate === "register") {
        const register_callback: string | undefined = await StorageGetItemAsync("RegisterCallback");

        if (register_callback !== undefined && register_callback !== "") {
            WebApp.close();
        }
        else {
            navigate("/");
        }
    }
    else if (validate === "logined") {
      setLogined(true);
    }
  }


  const TokenGetRequestAuth = async () => {
    if (WebApp.initDataUnsafe.user !== undefined) {
      tg_component = {
          id: WebApp.initDataUnsafe.user?.id,
          first_name: WebApp.initDataUnsafe.user?.first_name,
          last_name: WebApp.initDataUnsafe.user?.last_name === undefined ? null : WebApp.initDataUnsafe.user?.last_name,
          username: WebApp.initDataUnsafe.user?.username === undefined ? null : WebApp.initDataUnsafe.user?.username,
          is_bot: WebApp.initDataUnsafe.user?.is_bot === undefined ? false : WebApp.initDataUnsafe.user?.is_bot,
          photo_url: WebApp.initDataUnsafe.user?.photo_url === undefined ? null : WebApp.initDataUnsafe.user?.photo_url,
          chat_id: WebApp.initDataUnsafe.user?.id,
          address: "",
          device: (WebApp.platform === 'ios' || WebApp.platform === 'android') ? "Mobile" : "PC",
          roles: ["Client"]
      } 
    }
  }

  const AuthCheck = async (withoutNB: boolean = false) => {

    const accessToken: string | undefined = await StorageGetItemAsync('AccessToken');

    const register_callback: string | undefined = await StorageGetItemAsync("RegisterCallback");

    if (accessToken === "" && register_callback !== "register_requested" && !withoutNB) {
        navigate("/onboarding")
    }

    if (accessToken !== undefined) {
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

    AuthCheck();
  }, []);

  useEffect(() => {
    if (logined)
      AuthCheck(true);
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

                {userInfo !== null && <>
                  <div className="app_delivery_header">
                    <div className="app_delivery_header_image"></div>

                    <div className="app_delivery_header_title_area">
                        <div className="app_delivery_header_title">{userInfo.address}</div>
                        <div className="app_delivery_header_title big">Адрес доставки</div>
                    </div>

                    <div className="app_delivery_header_profile"  style={{
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
