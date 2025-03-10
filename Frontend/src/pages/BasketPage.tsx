import { useEffect, useState } from 'react'
import '../styles/AppStyle.sass'
import WebApp from '@twa-dev/sdk';
import { useNavigate, useLocation, data } from 'react-router-dom';
import { handleGetInfoMe } from '../api-integrations/ClientInfoAPI';
import { StorageGetItem } from '../telegram-integrations/cloudstorage/CloudStorage';
import { GetBasketInfo, GetMeInfo } from '../api-integrations/Interfaces/API_Interfaces';
import { BackButton } from '@twa-dev/sdk/react';
import { handleBasketDeleteItem, handleGetBasketInfo } from '../api-integrations/BasketAPI';
import { handleLoadImage } from '../api-integrations/ImageAPI';

const BasketItem: React.FC<{imageLink: string, name: string, price: number, itemid: string, onDeleted: () => void }> = ({imageLink, name, price, itemid, onDeleted}) => {

    const [imageRendered, setImageRendered] = useState<string | null>(null);

    const PostDeleteBasketItemRequest = async (basketId: string, accessToken: string) => {
        const deletedItem = await handleBasketDeleteItem(basketId, accessToken);

        if (deletedItem) {
            onDeleted();
        } 
    }

    const renderImage = async () => {
        const imageItem = await handleLoadImage(imageLink);

        if (imageItem !== null)
            setImageRendered(imageItem);
    }

    useEffect(() => {
        renderImage();

    }, [])

    const DeleteBasketItem = async (basketId: string) => {
        const accessToken: string = await StorageGetItem('AccessToken');
    
        if (accessToken !== "empty") {
          await PostDeleteBasketItemRequest(basketId, accessToken);
        }
    }

    return (<>
            <div className="app_basket_item">
                <div className="app_basket_item_image" style={{
                    backgroundImage: `url(${imageRendered})`
                }}>

                </div>

                <div className="app_basket_item_title">
                    {`${name}`}
                </div>

                <div className="app_basket_item_title sm">
                    {`${price} руб.`}
                </div>

                <div className="app_basket_item_delete" 
                onClick={()=>DeleteBasketItem(itemid)}></div>

            </div>
        
    </>)
}

const BasketPage: React.FC = () => {

    const navigate = useNavigate();

    const [isMobile, setIsMobile] = useState<boolean>(false);
    
    const [userInfo, setUserInfo] = useState<GetMeInfo | null>(null);
  
    const [basketInfo, setBasketInfo] = useState<GetBasketInfo | null>(null);

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

                    <div className="app_maincontent_area" style={isMobile ? {height: 'calc(100% - 200px)'} : {}}>

                        {basketInfo !== null && <>

                            {basketInfo.basketItem.map((item, index) => <>

                                <BasketItem 
                                    key={index} 
                                    itemid={item.id} 
                                    imageLink={item.image}
                                    name={item.name}
                                    price={item.price}
                                    onDeleted={()=>setBasketInfo(null)}
                                />

                            </>)}

                            <div className="app_basket_separator"></div>
                        </>}

                    </div>

                    {basketInfo !== null && <>
                            <div className="app_basket_order_complete_area" style={
                                isMobile ? {marginBottom: 'calc(100px + 45px)'} : {}
                                }>
                                <div className="app_basket_order_complete_button" onClick={()=>navigate("/ordered")}>
                                    {`Заказ на ${basketInfo.basketInfo.totalPrice} руб`}
                                </div>
                            </div>
                        </>
                    }

                </div>
            </>}

            {(isMobile) && <div className="app_mobile_footer">Симбир Еда</div>}

            </div>
    
        </div>
    </>)
}

export default BasketPage;