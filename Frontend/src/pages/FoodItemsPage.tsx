import { useEffect, useRef, useState } from 'react';
import '../styles/AppStyle.sass';
import WebApp from '@twa-dev/sdk';
import { StorageGetItem, StorageSetItem, StorageDeleteItem } from '../telegram-integrations/cloudstorage/CloudStorage.ts';
import { useNavigate, useLocation, data } from 'react-router-dom';
import { loadingComponent } from '../LoadingComponent.ts';
import { BackButton } from '@twa-dev/sdk/react';
import { FoodItemInfo, GetBasketInfo, RestaurantInfo } from '../api-integrations/Interfaces/API_Interfaces.ts';
import { handleFoodItemsInfo, handleFoodItemsInfoWithSearch } from '../api-integrations/RestaurantAPI.ts';
import { motion } from "framer-motion";
import { handleBasketAddItem, handleGetBasketInfo } from '../api-integrations/BasketAPI.ts';
import { handleLoadImage } from '../api-integrations/ImageAPI.ts';

var loadingItems = new loadingComponent();

const FoodItemComponent: React.FC<{info: FoodItemInfo, onClickInner: () => void}> = ({info, onClickInner}) => {
    const containerRef = useRef<HTMLDivElement>(null);
    const textRef = useRef<HTMLDivElement>(null);
    const [isOverflowing, setIsOverflowing] = useState(false);
    const [textWidth, setTextWidth] = useState(0);
    const [containerWidth, setContainerWidth] = useState(0);
    const [duration, setDuration] = useState(5);
    const [addedItem, setAddedItem] = useState(false);

    const [imageRendered, setImageRendered] = useState<string | null>(null);

    const [runningA, setRunningA] = useState<boolean>(false);

    const renderImage = async () => {
        const accessToken: string = await StorageGetItem('AccessToken');

        if (accessToken !== "empty") {
            const imageItem = await handleLoadImage(accessToken, info.image);

            if (imageItem !== null)
                setImageRendered(imageItem);
        }
    }

    const checkOverflow = () => {
        if (containerRef.current && textRef.current) {
            const textW = textRef.current.scrollWidth;
            const containerW = containerRef.current.clientWidth;
            setTextWidth(textW);
            setContainerWidth(containerW);
            setIsOverflowing(textW > containerW);

            if (textW > containerW) {
                setDuration(textW / 50); 
            }
        }
    };


    useEffect(() => {
        checkOverflow();
        window.addEventListener("resize", checkOverflow);
        return () => window.removeEventListener("resize", checkOverflow);
    }, []);

    useEffect(() => {

        loadingItems.startLoading();
        loadingItems.startLoadingAnimation();

        renderImage();
    }, [])

    const AnimationChecker = () => {
        if (loadingItems.getStatusLoadingAnimation().isCompleted) {
            setRunningA(true);
        }
      }
    
      useEffect(()=>{
          if (!runningA) { 
              const intervalId = setInterval(()=>AnimationChecker(), 2000); 
    
              return () => clearInterval(intervalId);    
          }

          checkOverflow();
      }, [runningA])

  

    const AddBasketItemRequest = async (foodItemId: string, accessToken: string) => {

        const basketrequest = await handleBasketAddItem(foodItemId, accessToken);
    
        if (basketrequest === true) { //200
            setAddedItem(true);
        }
    
    }

    const AddBasketItem = async (foodItemId: string) => {
        const accessToken: string = await StorageGetItem('AccessToken');
    
        if (accessToken !== "empty") {
          await AddBasketItemRequest(foodItemId, accessToken);
        }
    }

    useEffect(() => {
        if (addedItem) {
           setAddedItem(false);
        }
    }, [addedItem])

    useEffect(()=> {
        if (imageRendered !== null) {
            loadingItems.endLoading();
         
        }
    }, [imageRendered])

    return (<>
        <div className="app_fooditem_content_item" >

            {(!runningA || loadingItems.getLoading()) && <>
                <div className="app_fooditem_content_item_image loading"></div>
                <div className="app_fooditem_content_item_price loading"></div>
                <div className="app_fooditem_content_item_name loading"></div>
            </>}

            {(runningA && !loadingItems.getLoading()) && <> 
                <div className="app_fooditem_content_item_image" style={{backgroundImage: `url(${imageRendered})`}}></div>
                <div className="app_fooditem_content_item_price">{`${info.price}Р`}</div>
                <div className="app_fooditem_content_item_name" ref={containerRef}>
                    <motion.div
                    ref={textRef}
                    animate={isOverflowing ? { x: [0, -(textWidth - containerWidth), -(textWidth - containerWidth), 0, 0] } : {}}
                    transition={isOverflowing ? { 
                        repeat: Infinity, 
                        duration: duration + 2 + 1, 
                        ease: "linear", 
                        times: [0, 0.45, 0.55, 1, 1], 
                        repeatDelay: 1
                    } : {}}
                    style={{ display: "inline-block" }}
                    >
                        {`${info.name}`}
                    </motion.div>
                
                </div>

            
                <div className="app_fooditem_content_item_weight">{info.weight > 0 ? `${info.weight}г` : ``}</div>

                <div className="app_fooditem_content_item_basketbutton" onClick={() => { AddBasketItem(info.id); onClickInner(); }}>
                    <div className="textdiv">
                    {`Добавить`}
                    </div>
        
                </div>
            </>}
 
           

        </div>
    </>)
}

const FoodItemsPage: React.FC = () => {

    const navigate = useNavigate();

    const locationReact = useLocation();

    const restaurantInfo: RestaurantInfo = locationReact.state?.restaurantInfo || '';

    const [fooditems, setFoodItemsInfo] = useState<FoodItemInfo[] | null>(null);

    const [isMobile, setIsMobile] = useState<boolean>(false);

    const [basketInfo, setBasketInfo] = useState<GetBasketInfo | null>(null);

    const [inputValue, setInputValue] = useState('');

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
    
    useEffect(()=> {
      WebApp.setHeaderColor('#EAEAEA');

      WebApp.setBackgroundColor('#004681');

      if (WebApp.platform === 'ios' || WebApp.platform === 'android')
          setIsMobile(true);
      else 
          setIsMobile(false);

      WebApp.ready();


    }, [])

    useEffect(()=>{
      if (basketInfo === null) {
        BasketGet();
      }
    }, [basketInfo])

    useEffect(()=>{
      if (isMobile) {
          WebApp.lockOrientation();
          WebApp.requestFullscreen();
      }
    }, [isMobile])

    useEffect(()=> {
      if (fooditems === null) {
          FoodItemsGet();
      }
    }, [fooditems])

    const GetFoodItemsAPI = async (accessToken: string) => {

        const fooditemsinfo = await handleFoodItemsInfo(accessToken, restaurantInfo.id);
    
        if (fooditemsinfo !== null && inputValue === '') {
            setFoodItemsInfo(fooditemsinfo);
        }
    
    }

    const GetBasketRequestAPI = async (accessToken: string) => {
        const getbasket = await handleGetBasketInfo(accessToken);

        if (getbasket !== null) {
            setBasketInfo(getbasket);
        }
    }

    const BasketGet = async () => {
        const accessToken: string = await StorageGetItem('AccessToken');
    
        if (accessToken !== "empty") {
          await GetBasketRequestAPI(accessToken);
        }
    }


    const FoodItemsGet = async () => {
        const accessToken: string = await StorageGetItem('AccessToken');
    
        if (accessToken !== "empty") {
          await GetFoodItemsAPI(accessToken);
        }
    }


    const GetFoodItemsWithSearchAPI = async (accessToken: string) => {

        const fooditemsinfo = await handleFoodItemsInfoWithSearch(accessToken, restaurantInfo.id, inputValue);
    
        if (fooditemsinfo !== null) {
            setFoodItemsInfo(fooditemsinfo);
        }
    
    }

    const FoodItemsGetSearched = async () => {
        const accessToken: string = await StorageGetItem('AccessToken');
    
        if (accessToken !== "empty") {
          await GetFoodItemsWithSearchAPI(accessToken);
        }
    }

    useEffect(()=> {
        if (inputValue !== '') {
          setFoodItemsInfo(null);
          FoodItemsGetSearched();
        }
        else {
          setFoodItemsInfo(null);
          FoodItemsGet();
        }
    }, [inputValue])

    return (<>
        <BackButton onClick={()=>navigate("/")}/>

        <div className="app_background_area">

            <div className="app_layout_area" style={ isMobile ? { marginTop: '100px' } : {}}>
            
            <div className="app_fooditem_header">
                <div className="app_fooditem_header_title">{`${restaurantInfo.restaurantName}`}</div>

                <div className="app_maincontent_restaurant_block_subtitle_area">
                    <div className="app_maincontent_restaurant_block_subtitle_mark">{`${restaurantInfo.average_mark}`}</div>
                    <div className="app_maincontent_restaurant_block_subtitle_markimg"></div>

                    {isTimeInRange(restaurantInfo.open_time, restaurantInfo.close_time) && <>
                        <div className="app_maincontent_restaurant_block_subtitle_opened">{'Открыто'}</div>
                    </>}

                    {!isTimeInRange(restaurantInfo.open_time, restaurantInfo.close_time) && <>
                        <div className="app_maincontent_restaurant_block_subtitle_closed">{'Закрыто'}</div>
                        <div className="app_maincontent_restaurant_block_subtitle_closed info">{`Открытие в ${restaurantInfo.open_time}`}</div>
                    </>}
                </div>
            </div>

            <div className="app_fooditem_content" style={isMobile ? {height: `calc(100vh - 240px)`} : {}}>
               
                <div className="app_maincontent_searchbar_decor restaurants fooditems" style={{marginBottom: "5px"}}>
                      <input className='app_maincontent_searchbar'
                          type="text"
                          value={inputValue}
                          onChange={(e) => setInputValue(e.target.value)}
                          placeholder={'Поиск'}
                          style={{backgroundColor: '#EAEAEA'}}
                      /> 

                      <div className="app_maincontent_searchbar_icon"></div>
                </div>

                <div className="app_fooditem_content_area" style={basketInfo !== null && basketInfo.basketInfo.count > 0 ? {paddingBottom: '70px', height: 'calc(100% - 55px - 70px)'} : {}}>
                    {fooditems !== null && fooditems.map((item, index) => <>
                        <FoodItemComponent key={index} info={item} onClickInner={() => setBasketInfo(null)}/>
                    </>)}
                </div>
            </div>

      
            {basketInfo !== null && basketInfo.basketInfo.count > 0 && <>
                <div className="app_basket_order_complete_area" style={
                    isMobile ? {marginBottom: 'calc(100px + 45px)'} : {}
                    }>
                    <div className="app_basket_order_complete_button" onClick={()=>navigate("/basket")}>
                        {`Корзина на ${basketInfo.basketInfo.totalPrice} руб`}
                    </div>
                </div>
                </>
            }
     
            
            {(isMobile) && <div className="app_mobile_footer">Симбир Еда</div>}
            </div>
        </div>
    </>)
}

export default FoodItemsPage;