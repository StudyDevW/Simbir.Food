import { useEffect, useState } from 'react'
import '../../styles/AppStyle.sass'
import WebApp from '@twa-dev/sdk';
import { StorageGetItemAsync, StorageSetItem, StorageDeleteItem } from '../../cloudstorage-telegram/CloudStorage.ts';
import { useNavigate, useLocation } from 'react-router-dom';

const OnBoardingPageFirst: React.FC = () => {

  const [preLoad, setPreLoad] = useState<boolean>(false);

  useEffect(()=>{
    if (!preLoad) {
        const interval = setInterval(() => {
          setPreLoad(true);
        }, 500); 

        return () => clearInterval(interval);
    }
  }, [preLoad])

  return (<>
    <div className="app_onboarding_title">
      Добро пожаловать<br></br>
      в сервис быстрой доставки<br></br>
      еды из ресторанов
    </div>

    <div className="app_onboarding_restaurant_area" style={preLoad ? {animation: 'app_onboarding_restaurant_anim 1s forwards'} : {}}>

      <div className="app_onboarding_restaurant">
        <div className="app_onboarding_restaurant_image" style={{backgroundImage: 'url(./images/vkusno.jpg)'}}></div>

        <div className="app_onboarding_restaurant_title">Вкусно - и точка</div>
        
        <div className="app_onboarding_restaurant_more_area">
        
          <div className="app_onboarding_restaurant_score">4.9</div>

          <div className="app_onboarding_restaurant_scoreimage"></div>

          <div className="app_onboarding_restaurant_statusopened">Открыто</div>

        </div>
        
      </div>

      <div className="app_onboarding_restaurant">

        <div className="app_onboarding_restaurant_image" style={{backgroundImage: 'url(./images/burgerking.jpg)'}}></div>

        <div className="app_onboarding_restaurant_title">Бургер Кинг</div>

        <div className="app_onboarding_restaurant_more_area">

          <div className="app_onboarding_restaurant_score">4.5</div>

          <div className="app_onboarding_restaurant_scoreimage"></div>

          <div className="app_onboarding_restaurant_statusopened">Открыто</div>

        </div>

      </div>

    </div>

    <div className="app_onboarding_title reversed">
      Выбирайте нужный ресторан<br></br>
      и курьер принесет заказ<br></br>
      за считанные минуты
    </div>

    <div className="app_onboarding_delivery_area">
      <div className="app_onboarding_delivery_back" style={preLoad ? {animation: 'app_onboarding_restaurant_anim 1.2s forwards'} : {}}>
        <div className="app_onboarding_delivery_item" style={{backgroundImage: 'url(./images/courier_2.png)'}}></div>
        <div className="app_onboarding_delivery_item" style={{backgroundImage: 'url(./images/courier_1.png)'}}></div>
      </div>

      <div className="app_onboarding_logo" style={preLoad ? {animation: 'app_onboarding_restaurant_anim 0.7s forwards'} : {}}></div>
    </div>
  </>)
}

const OnBoardingPageSecond: React.FC = () => {

  const [preLoad, setPreLoad] = useState<boolean>(false);

  useEffect(()=>{
    if (!preLoad) {
        const interval = setInterval(() => {
          setPreLoad(true);
        }, 600); 

        return () => clearInterval(interval);
    }
  }, [preLoad])
  

  return (<>
    <div className="app_onboarding_title">
      Добавляйте свои любимые<br></br>
      рестораны в избранное!
    </div>

    <div className="app_onboarding_restaurant_area fullsize" style={preLoad ? {
      animation: 'app_onboarding_restaurant_anim 1s forwards'
      } : {}}>

      <div className="app_onboarding_restaurant fullsize">
        <div className="app_onboarding_restaurant_image" style={{backgroundImage: 'url(./images/vkusno.jpg)'}}></div>

        <div className="app_onboarding_restaurant_featured_area" style={preLoad ? {
        animation: 'app_onboarding_scaling 1s alternate infinite'
        } : {}}>
          <div className="app_onboarding_restaurant_featured_text">Добавить в</div>
          <div className="app_onboarding_restaurant_featured_image"></div>
        </div>

        <div className="app_onboarding_restaurant_title fullsize">Вкусно - и точка</div>
        
        <div className="app_onboarding_restaurant_more_area fullsize">
        
          <div className="app_onboarding_restaurant_score fullsize">4.9</div>

          <div className="app_onboarding_restaurant_scoreimage fullsize"></div>

          <div className="app_onboarding_restaurant_statusopened" style={{fontSize: '12px'}}>Открыто</div>

        </div>
        
      </div>

    </div>

    <div className="app_onboarding_arrow_fill" style={preLoad ? {
      animation: 'app_onboarding_arrow 1s alternate infinite'
      } : {}}></div>  

    <div className="app_onboarding_featured_heart" style={preLoad ? {
      animation: 'app_onboarding_restaurant_anim 0.7s forwards'
      } : {}}>
      <div className="app_onboarding_featured_logo"></div>
    </div>  

  </>)
}

const OnBoardingPageThird: React.FC = () => {

  const [preLoad, setPreLoad] = useState<boolean>(false);

  useEffect(()=>{
    if (!preLoad) {
        const interval = setInterval(() => {
          setPreLoad(true);
        }, 600); 

        return () => clearInterval(interval);
    }
  }, [preLoad])
  

  return (<>
    <div className="app_onboarding_title">
      Сверяйте рестораны<br/>
      по их оценкам и отзывам
    </div>

    <div className="app_onboarding_restaurant_prev_area">
      <div className="app_onboarding_restaurant_prev_item_area" style={preLoad ? {animation: 'app_onboarding_restaurant_anim 1.0s forwards'} : {}}>
        
        <div className="app_onboarding_restaurant_prev_item"  style={preLoad ? {animation: 'app_onboarding_restaurant_anim 1.3s forwards'} : {}}>

          <div className="app_onboarding_restaurant_title fullsize">Ресторан</div>
          
          <div className="app_onboarding_restaurant_more_area">
          
            <div className="app_onboarding_restaurant_score fullsize">4.3</div>

            <div className="app_onboarding_restaurant_scoreimage fullsize"></div>

            <div className="app_onboarding_restaurant_statusopened" style={{fontSize: '12px'}}>Открыто</div>

          </div>

        </div>

        <div className="app_onboarding_restaurant_prev_item" style={preLoad ? {animation: 'app_onboarding_restaurant_anim 1.5s forwards'} : {}}>

          <div className="app_onboarding_restaurant_title fullsize">Ресторан</div>
          
          <div className="app_onboarding_restaurant_more_area">
          
            <div className="app_onboarding_restaurant_score fullsize">3.0</div>

            <div className="app_onboarding_restaurant_scoreimage fullsize"></div>

            <div className="app_onboarding_restaurant_statusopened" style={{fontSize: '12px', color: 'rgb(150, 0, 0)'}}>Закрыто</div>

          </div>

        </div>
        
        <div className="app_onboarding_restaurant_prev_item" style={preLoad ? {animation: 'app_onboarding_restaurant_anim 1.6s forwards'} : {}}>

          <div className="app_onboarding_restaurant_title fullsize">Ресторан</div>
          
          <div className="app_onboarding_restaurant_more_area" >
          
            <div className="app_onboarding_restaurant_score fullsize" style={preLoad ? {
            animation: 'app_onboarding_scaling 1s alternate infinite'
            } : {}}>5.0</div>

            <div className="app_onboarding_restaurant_scoreimage fullsize" style={preLoad ? {
            animation: 'app_onboarding_scaling 1s alternate infinite'
            } : {}}></div>

            <div className="app_onboarding_restaurant_statusopened" style={{fontSize: '12px'}}>Открыто</div>

          </div>

        </div>

  
      </div>

      <div className="app_onboarding_logo_r" style={preLoad ? {animation: 'app_onboarding_restaurant_anim 0.7s forwards'} : {}}></div>


    </div>

    <div className="app_onboarding_arrow_rest" style={preLoad ? {
      animation: 'app_onboarding_arrow_new 1s alternate infinite'
      } : {}}>

    </div>

    <div className="app_onboarding_arrow_text_area" style={preLoad ? {animation: 'app_onboarding_title_animrev 1s forwards'} : {}}>
      <div className="app_onboarding_arrow_text">Отличный ресторан</div>
      <div className="app_onboarding_arrow_image"></div>
    </div>

  </>)
}

const OnBoardingSearchBarAnimation: React.FC<{text: string}> = ({ text }) => {
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

  return <div className="app_onboarding_searchbar_text">{displayedText}</div>;
};

const OnBoardingPageFour: React.FC = () => {

  const [preLoad, setPreLoad] = useState<boolean>(false);

  const [postLoad, setPostLoad] = useState<boolean>(false);

  useEffect(()=>{
    if (!preLoad) {
        const interval = setInterval(() => {
          setPreLoad(true);
        }, 600); 

        return () => clearInterval(interval);
    }
  }, [preLoad])

  useEffect(()=>{
    if (!postLoad) {
        const interval = setInterval(() => {
          setPostLoad(true);
        }, 2200); 

        return () => clearInterval(interval);
    }
  }, [postLoad])

  return (<>
    <div className="app_onboarding_title" style={{textAlign: 'center'}}>
      Удобный поиск ресторанов
    </div>

    <div className="app_onboarding_searchbar_area">
      <div className="app_onboarding_searchbar" style={preLoad ? {
      animation: 'app_onboarding_restaurant_anim 0.7s forwards'
      } : {}}>
        <div className="app_onboarding_searchbar_icon"></div>
        {preLoad && <OnBoardingSearchBarAnimation text='Хочу есть!'/>}

      </div>
    </div>

    <div className="app_onboarding_rest_area" style={{marginTop: '15px'}}>
      <div className="app_onboarding_rest" style={postLoad ? {animation: 'app_onboarding_restaurant_anim 1.1s forwards'} : {}}>
        
          <div className="app_onboarding_restaurant_title fullsize">Ресторан</div>
          
          <div className="app_onboarding_restaurant_more_area">
          
            <div className="app_onboarding_restaurant_score fullsize">4.3</div>

            <div className="app_onboarding_restaurant_scoreimage fullsize"></div>

            <div className="app_onboarding_restaurant_statusopened" style={{fontSize: '12px'}}>Открыто</div>

          </div>

          <div className="app_onboarding_restaurant_more_like">
  
          </div>

      </div>
    </div>

    <div className="app_onboarding_rest_area">
      <div className="app_onboarding_rest" style={postLoad ? {animation: 'app_onboarding_restaurant_anim 1.3s forwards'} : {}}>
        
          <div className="app_onboarding_restaurant_title fullsize">Ресторан</div>
          
          <div className="app_onboarding_restaurant_more_area">
          
            <div className="app_onboarding_restaurant_score fullsize">4.9</div>

            <div className="app_onboarding_restaurant_scoreimage fullsize"></div>

            <div className="app_onboarding_restaurant_statusopened" style={{fontSize: '12px'}}>Открыто</div>

          </div>

          <div className="app_onboarding_restaurant_more_like">
  
          </div>

      </div>
    </div>

    <div className="app_onboarding_rest_area">
      <div className="app_onboarding_rest" style={postLoad ? {animation: 'app_onboarding_restaurant_anim 1.5s forwards'} : {}}>
        
          <div className="app_onboarding_restaurant_title fullsize">Ресторан</div>
          
          <div className="app_onboarding_restaurant_more_area">
          
            <div className="app_onboarding_restaurant_score fullsize">5.0</div>

            <div className="app_onboarding_restaurant_scoreimage fullsize"></div>

            <div className="app_onboarding_restaurant_statusopened" style={{fontSize: '12px'}}>Открыто</div>

          </div>

          <div className="app_onboarding_restaurant_more_like">
  
          </div>

      </div>
    </div>

    <div className="app_onboarding_rest_area">
      <div className="app_onboarding_rest" style={postLoad ? {animation: 'app_onboarding_restaurant_anim 1.7s forwards'} : {}}>
        
          <div className="app_onboarding_restaurant_title fullsize">Ресторан</div>
          
          <div className="app_onboarding_restaurant_more_area">
          
            <div className="app_onboarding_restaurant_score fullsize">4.8</div>

            <div className="app_onboarding_restaurant_scoreimage fullsize"></div>

            <div className="app_onboarding_restaurant_statusopened" style={{fontSize: '12px'}}>Открыто</div>

          </div>

          <div className="app_onboarding_restaurant_more_like">
  
          </div>

      </div>
    </div>

  </>)
}

const OnBoardingMain: React.FC = () => {

  const navigate = useNavigate();

  const [isMobile, setIsMobile] = useState<boolean>(false);

  const [currentPage, setCurrentPage] = useState<number>(1);

  const [rightSwapped, setRightSwapped] = useState<boolean>(false);

  const [leftSwapped, setLeftSwapped] = useState<boolean>(false);

  const OnBoardingSkip = () => {
     WebApp.CloudStorage.setItem('onboarding-storage', "skipped");
     navigate("/login");
  }

  useEffect(() => {
    WebApp.setHeaderColor('#004681');

    WebApp.setBackgroundColor('#004681');

    WebApp.disableVerticalSwipes();

    if (WebApp.platform === 'ios' || WebApp.platform === 'android')
      setIsMobile(true);
    else 
      setIsMobile(false);

    StorageDeleteItem("RegisterCallback");

  }, []);

  useEffect(()=>{
    if (isMobile) {
        WebApp.lockOrientation();
        WebApp.requestFullscreen();
    }
  }, [isMobile])

  useEffect(()=> {
    if (rightSwapped) {
      setCurrentPage(prev => prev + 1);
      setRightSwapped(false);
    }
  }, [rightSwapped])

  useEffect(()=>{
    if (leftSwapped) {
      setCurrentPage(prev => prev - 1);
      setLeftSwapped(false);
    }
  }, [leftSwapped])

  const PageFillUp = (current_num: number) => {
    if (currentPage === current_num) 
      return 'app_onboarding_carusel_page active';

    return 'app_onboarding_carusel_page';
  }

  const PageSkip = () => {
    setCurrentPage(prev => prev + 1)

    if (currentPage > 3) 
        setCurrentPage(1);
  }

  const handleTouchStart = (e: React.TouchEvent<HTMLDivElement>) => {

    if (!isMobile)
      return;

    const startX = e.touches[0].clientX;

    const handleTouchMove = (e: TouchEvent) => {
      const moveX = e.touches[0].clientX;
      const diffX = startX - moveX;

      if (Math.abs(diffX) > 30) {
        if (diffX > 0) {
          if (currentPage < 4)
            setRightSwapped(true);
        } else {
            if (currentPage > 1)
              setLeftSwapped(true);
        }
        document.removeEventListener('touchmove', handleTouchMove);
      }
    };

    document.addEventListener('touchmove', handleTouchMove);
  };

  // const LoadingDraw = () => {
  //   return (<>
  //       <div className="app_loading_area" style={ isMobile ? { height: 'calc(100% - 100px - 45px)' } : {} }>
  //           <div className="app_loading_letter">
  //               <div className="app_loading_bar"></div>
  //           </div>
  //       </div>
  //   </>)
  // }

  return (
    <>
     
          <div className="app_background_area" style={{backgroundColor: '#004681' }}>

              <div className="app_layout_area" onTouchStart={handleTouchStart} style={ isMobile ? { marginTop: '100px', maxWidth: '550px', height: 'calc(100% - 100px)' } : { maxWidth: '550px'}}>
          
                 

                  {(currentPage === 1) && <OnBoardingPageFirst/>} 
                  
                  {(currentPage === 2) && <OnBoardingPageSecond/>} 

                  {(currentPage === 3) && <OnBoardingPageThird/>}

                  {(currentPage === 4) && <OnBoardingPageFour/>}

                  <div className="app_onboarding_carusel_area" style={ isMobile ? {bottom: '45px'} : {}}>

                    <div className="app_onboarding_carusel_left"
                    onClick={OnBoardingSkip}>Пропустить</div>

                    <div className="app_onboarding_carusel_center">
                      <div className={PageFillUp(1)} onClick={() => setCurrentPage(1)}></div>
                      <div className={PageFillUp(2)} onClick={() => setCurrentPage(2)}></div>
                      <div className={PageFillUp(3)} onClick={() => setCurrentPage(3)}></div>
                      <div className={PageFillUp(4)} onClick={() => setCurrentPage(4)}></div>
                    </div>

                    <div className="app_onboarding_carusel_right" onClick={PageSkip}></div>
                    
                  </div>

                  {(isMobile) && <div className="app_mobile_footer">Симбир Еда</div>}
            
              </div>
                
          </div>
       
    </>
  )
}

export default OnBoardingMain
