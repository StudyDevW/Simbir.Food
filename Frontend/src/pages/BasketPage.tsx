import { useEffect, useState } from 'react'
import '../styles/AppStyle.sass'
import WebApp from '@twa-dev/sdk';
import { useNavigate, useLocation, data } from 'react-router-dom';
import { handleGetInfoMe } from '../api-integrations/ClientInfoAPI';
import { StorageGetItem } from '../telegram-integrations/cloudstorage/CloudStorage';
import { GetBasketInfo, GetMeInfo } from '../api-integrations/Interfaces/API_Interfaces';
import { BackButton } from '@twa-dev/sdk/react';
import { handleBasketDeleteItem, handleGetBasketInfo } from '../api-integrations/BasketAPI';


const BasketPage: React.FC = () => {

    const navigate = useNavigate();

    const [isMobile, setIsMobile] = useState<boolean>(false);
    
    const [userInfo, setUserInfo] = useState<GetMeInfo | null>(null);
  
    const [basketInfo, setBasketInfo] = useState<GetBasketInfo | null>(null);

    const [logined, setLogined] = useState<boolean>(false);
  
    const [profileOpened, setProfileOpened] = useState<boolean>(false);

    useEffect(()=>{
        WebApp.setHeaderColor('#EAEAEA');

        WebApp.setBackgroundColor('#004681');
    
        if (WebApp.platform === 'ios' || WebApp.platform === 'android')
          setIsMobile(true);
        else 
          setIsMobile(false);
    
        WebApp.ready();
    
        ProfileGet();
    
       

    }, [])

    useEffect(()=>{
        if (basketInfo === null) {
            BasketGet();
        }
    }, [basketInfo])

    const GetBasketRequestAPI = async (accessToken: string) => {
        const getbasket = await handleGetBasketInfo(accessToken);

        if (getbasket !== null) {
            setBasketInfo(getbasket);
        }
    }

    const PostDeleteBasketItemRequest = async (basketId: string, accessToken: string) => {
        const deletedItem = await handleBasketDeleteItem(basketId, accessToken);

        if (deletedItem) {
            setBasketInfo(null);
        } 
    }

    const GetUserRequestAPI = async (accessToken: string) => {
    
        const getuser = await handleGetInfoMe(accessToken);
        
        if (getuser !== null) {
          setUserInfo(getuser);
        }
    }

    const ProfileGet = async () => {
        const accessToken: string = await StorageGetItem('AccessToken');
    
        if (accessToken !== "empty") {
          await GetUserRequestAPI(accessToken);
        }
    }

    const BasketGet = async () => {
        const accessToken: string = await StorageGetItem('AccessToken');
    
        if (accessToken !== "empty") {
          await GetBasketRequestAPI(accessToken);
        }
    }

    const DeleteBasketItem = async (basketId: string) => {
        const accessToken: string = await StorageGetItem('AccessToken');
    
        if (accessToken !== "empty") {
          await PostDeleteBasketItemRequest(basketId, accessToken);
        }
    }

    const LoadingDraw = () => {
        return (<>
            <div className="app_loading_area" style={ isMobile ? { height: 'calc(100% - 100px - 45px)' } : {} }>
                <div className="app_loading_letter">
                    <div className="app_loading_bar"></div>
                </div>
            </div>
        </>)
    }

    return (<>
        <BackButton onClick={()=>navigate("/")}/>

        <div className="app_background_area">

            <div className="app_layout_area" style={ isMobile ? { marginTop: '100px' } : {}}>

            {userInfo === null && LoadingDraw()}

            {userInfo !== null && <>

                <div className="app_maincontent" style={{height: '100%'}}>
                    <div className="app_maincontent_title">Корзина</div>

                    <div className="app_maincontent_area">

                        {basketInfo !== null && <>
                            {basketInfo.basketItem.map((item, index) => <>
                                <div key={index} className="app_basket_item">
                                    <div className="app_basket_item_image">

                                    </div>

                                    <div className="app_basket_item_title">
                                        {`${item.name}`}
                                    </div>

                                    <div className="app_basket_item_title sm">
                                        {`${item.price} руб.`}
                                    </div>

                                    <div className="app_basket_item_delete" 
                                    onClick={()=>DeleteBasketItem(item.id)}></div>

                                </div>
                            </>)}
                        </>}

                        

            

                    </div>
                </div>
            </>}

            </div>
        </div>
    </>)
}

export default BasketPage;