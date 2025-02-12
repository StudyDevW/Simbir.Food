import { useEffect, useState } from 'react'
import '../styles/AppStyle.sass'
import WebApp from '@twa-dev/sdk';

import { useNavigate, useLocation } from 'react-router-dom';

const MainPage: React.FC = () => {

  const navigate = useNavigate();

  const [isMobile, setIsMobile] = useState<boolean>(false);

  const [OnBoardingSlide, setOnBoardingSlide] = useState<boolean>(true);

  useEffect(() => {
    WebApp.setHeaderColor('#EAEAEA');

    WebApp.setBackgroundColor('#004681');

    if (WebApp.platform === 'ios' || WebApp.platform === 'android')
      setIsMobile(true);
    else 
      setIsMobile(false);

    if (OnBoardingSlide) {
      navigate("/onboarding")
      setOnBoardingSlide(false);
    }

  }, []);

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

                <div className="app_delivery_header">
                    <div className="app_delivery_header_image"></div>

                    <div className="app_delivery_header_title_area">
                        <div className="app_delivery_header_title">г.Ульяновск, ул. Энгельса, д.3</div>
                        <div className="app_delivery_header_title big">Адрес доставки</div>
                    </div>

                    <div className="app_delivery_header_profile"></div>
                </div>


                {LoadingDraw()}

                {/* <div className="app_maincontent">
                    <div className="app_maincontent_title">Рестораны</div>

                    

                </div> */}

                {(isMobile) && <div className="app_mobile_footer">@SimbirFoodbot</div>}

            </div>

        </div>
    </>
  )
}

export default MainPage
