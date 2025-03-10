import { useEffect, useRef, useState } from "react";

import WebApp from '@twa-dev/sdk';
import { BackButton } from '@twa-dev/sdk/react';
import { useNavigate, useLocation, data } from 'react-router-dom';

const PaymentPage: React.FC = () => {
    const navigate = useNavigate();

    const [numberCard, setNumberCard] = useState<string>("");
    const [payChecked, setPayChecked] = useState<boolean>(false);
    const [numberCVV, setNumberCVV] = useState<string>("");
    const [cardMaxNum, setCardMaxNum] = useState<boolean>(false);
    
    const [isMobile, setIsMobile] = useState<boolean>(false);
  
    const inputRef1 = useRef<HTMLInputElement>(null);
    const inputRef2 = useRef<HTMLInputElement>(null);

    const insertSpacesEveryFourChars = (input: string): string => {
        return input.replace(/(.{4})/g, '$1 ').trim();
    }

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
    
        WebApp.ready();

    }, [])

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value.replace(/\s+/g, '');
        const formattedValue = insertSpacesEveryFourChars(value);
        setNumberCard(formattedValue);
    };

    const CardMaxNumberAnimation = (classname: string) => {
        if (cardMaxNum) 
          return `${classname} active`
        
  
        return `${classname}`
    }

    return (<>
       <BackButton onClick={()=>navigate("/")}/>

       <div className="app_background_area">

            <div className="app_layout_area" style={ isMobile ? { marginTop: '100px' } : {}}>

            <div className="area_payment">
                    <div className="payment_title">
                    Пополнение баланса
                    </div>
        
            
                    <div className="payment_area_elements">
            
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
                            onKeyDown={(e) => handleKeyDownBack(e, inputRef1)}></input>
            
                        <div className="secondcardpay_desc">Код CVV</div>
                        <div className="image_desc left">*обратная сторона карты</div>
                        </div>

                        {(numberCVV.length === 3) && <div className="buy_button">
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