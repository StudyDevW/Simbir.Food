import { useEffect, useRef, useState } from "react";

import WebApp from '@twa-dev/sdk';
import { BackButton } from '@twa-dev/sdk/react';
import { useNavigate, useLocation, data } from 'react-router-dom';
import { handlePayOperate } from "../api-integrations/PaymentAPI";
import { GetMeInfo, PaymentInfo } from "../api-integrations/Interfaces/API_Interfaces";
import { handleGetInfoMe } from "../api-integrations/ClientInfoAPI";
import { StorageGetItem } from "../vk-integrations/cloudstorage/CloudStorage";

const PaymentPage: React.FC = () => {
    const navigate = useNavigate();

    const locationReact = useLocation();

    
    const money_to_up = locationReact.state?.money_to_up || 0;

    const redirect_order = locationReact.state?.redirect_to_order || null;

    const [numberCard, setNumberCard] = useState<string>("");
    const [payChecked, setPayChecked] = useState<boolean>(false);
    const [numberCVV, setNumberCVV] = useState<string>("");
    const [cardMaxNum, setCardMaxNum] = useState<boolean>(false);

    const [userInfo, setUserInfo] = useState<GetMeInfo | null>(null);
  
    const [isMobile, setIsMobile] = useState<boolean>(false);
  
    const divRef = useRef<HTMLDivElement>(null);

    const inputRef1 = useRef<HTMLInputElement>(null);
    const inputRef2 = useRef<HTMLInputElement>(null);

    const insertSpacesEveryFourChars = (input: string): string => {
        return input.replace(/(.{4})/g, '$1 ').trim();
    }

    const resetViewport = () => {
        const viewportMetaTag = document.querySelector('meta[name="viewport"]');
        if (viewportMetaTag) {
          viewportMetaTag.setAttribute('content', 'width=device-width, initial-scale=1.0');
        }
    };

    const handleKeyDown = (nextInputRef: React.RefObject<HTMLInputElement | null>) => {
        nextInputRef.current?.focus();
    };
  
    const handleKeyDownBack = (e: React.KeyboardEvent<HTMLInputElement | null>, nextInputRef: React.RefObject<HTMLInputElement | null>) => {
      if (e.key === 'Backspace' && numberCVV.length === 0)
        nextInputRef.current?.focus();
    };
  
    useEffect(()=>{
        if (numberCard.length === 19) {
          setCardMaxNum(true);

          handleKeyDown(inputRef2);
        }
        else {
          setNumberCVV("")
          setCardMaxNum(false);
        }
    }, [numberCard])

    

    useEffect(()=>{
        WebApp.setHeaderColor('#EAEAEA');

        WebApp.setBackgroundColor('#004681');
    
        if (WebApp.platform === 'ios' || WebApp.platform === 'android')
          setIsMobile(true);
        else 
          setIsMobile(false);
    
        WebApp.disableVerticalSwipes();

        WebApp.ready();
    }, [])

    const ProfileGet = async () => {
        const accessToken: string = await StorageGetItem('AccessToken');
    
        if (accessToken !== "empty") {
          await GetUserRequestAPI(accessToken);
        }
    }
    

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value.replace(/\s+/g, '');
        const formattedValue = insertSpacesEveryFourChars(value);
        setNumberCard(formattedValue);
    };

    const CardMaxNumberAnimation = (classname: string) => {
        if (cardMaxNum && !payChecked) 
          return `${classname} active`
        
  
        return `${classname}`
    }


    const GetUserRequestAPI = async (accessToken: string) => {
      
      const getuser = await handleGetInfoMe(accessToken);
      
      if (getuser !== null) {
        setUserInfo(getuser);
      }
    }

    useEffect(()=> {
        if (userInfo === null) {
            ProfileGet();
        }
    }, [userInfo])

    const PayOperateAPI = async () => {

        if (userInfo === null) {
            alert("userInfo = null");
        }
        else {
            if (userInfo.id !== undefined) {
                
                const paymentInformation: PaymentInfo = {
                    card_number: numberCard,
                    cvv: numberCVV,
                    link_card: false,
                    money_value: money_to_up,
                    user_id: userInfo.id
                }

                var pay = await handlePayOperate(paymentInformation);

                if (pay) {
                    WebApp.showAlert("Счет успешно пополнен!");

                    if (redirect_order === true) {
                        navigate("/ordered");
                    }
                    else 
                        navigate("/");
                }
                else {
                    WebApp.showAlert("Операция прошла неуспешна");
                    if (redirect_order === true) {
                        navigate("/ordered");
                    }
                    else 
                        navigate("/");
                }
            }
            else {
                alert("userInfo.id = undefined")
            }
        }
    } 

    return (<>
       <BackButton onClick={()=>navigate("/")}/>

       <div className="app_background_area">

            <div ref={divRef} className="app_layout_area" style={ isMobile ? { top: '100px', height: '400px' } : {}}>

            <div className="area_payment" style={isMobile ? { position: 'fixed', top: '100px'} : { top: '0px'}}>
                    <div  className="payment_title">
                    Пополнение баланса
                    </div>
        
            
                    <div className="payment_area_elements" style={isMobile ? { top: '-50px'} : {}}>
            
                    {/* <div className="payment_cards_fromother_area">
                        <div className="payment_cards_fromother_title">Привязанные карты</div>

                        <div className="payment_cards_fromother_card_item_area">

                            <div className="payment_cards_fromother_card_item">
                            <div className="payment_cards_fromother_card_item_number" style={{fontSize: '16px', marginTop: '50px', marginLeft: '-1px', userSelect: 'none'}}>Оплатить другой картой</div>
                            </div>

                            <div className="payment_cards_fromother_card_item">
                            <div className="payment_cards_fromother_card_item_title">Виртуальная карта</div>
                            <div className="payment_cards_fromother_card_item_number">0000 1234 0000 4321</div>
                            <div className="payment_cards_fromother_card_item_image_desc"></div>
                            </div>

                            <div className="payment_cards_fromother_card_item">
                            <div className="payment_cards_fromother_card_item_title">Виртуальная карта</div>
                            <div className="payment_cards_fromother_card_item_number">0000 1234 0000 4321</div>
                            <div className="payment_cards_fromother_card_item_image_desc"></div>
                            </div>

                        </div>
                    </div> */}

                        <div className={CardMaxNumberAnimation("firstcardpay")}>
                        <div className="firstcardpay_title">Виртуальная карта</div>
                        <input  className="input_text" 
                            type="text" maxLength={19}
                            value={numberCard} ref={inputRef1}
                            onChange={handleChange}></input>
            
                        <div className="firstcardpay_desc">Введите номер карты</div>
            
                        <div className="image_desc"></div>
                        </div>
            
                        <div className={CardMaxNumberAnimation("secondcardpay")}>
                        <div className="secondcardpay_line"></div>
            
                        <input  className="input_text small" 
                            type="password" maxLength={3} 
                            value={numberCVV} ref={inputRef2}
                            onChange={(e) => setNumberCVV(e.target.value)}
                            onKeyDown={(e) => handleKeyDownBack(e, inputRef1)}
                            ></input>
            
                        <div className="secondcardpay_desc">Код CVV</div>
                        <div className="image_desc left">*обратная сторона карты</div>
                        </div>

                        {(numberCVV.length === 3) && <div className="buy_button" 
                        onClick={() =>{ handleKeyDown(inputRef1); PayOperateAPI();}}>
                            Оплатить
                        </div>} 

                    {/* 
            
                    */}
            
                    </div>
            
                </div>
            </div>
        </div>
    </>)
}

export default PaymentPage;