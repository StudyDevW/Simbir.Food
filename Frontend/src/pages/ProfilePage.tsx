import WebApp from "@twa-dev/sdk";
import { useEffect, useState } from "react";


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

const ProfilePage: React.FC<{info: GetMeInfo, isMobile: boolean, onChange: (newValue: boolean) => void}> = ({info, isMobile, onChange}) => {

    const [closedProfile, setClosedProfile] = useState<boolean>(false);

    useEffect(() => {
        WebApp.disableVerticalSwipes();
    }, [])

    
    const handleTouchStart = (e: React.TouchEvent<HTMLDivElement>) => {

        if (!isMobile)
            return;

        const startX = e.touches[0].clientX;

        const handleTouchMove = (e: TouchEvent) => {
        const moveX = e.touches[0].clientX;
        const diffX = startX - moveX;

        if (Math.abs(diffX) > 30) {
            if (diffX < 0) {
                setClosedProfile(true);
            } 

            document.removeEventListener('touchmove', handleTouchMove);
        }
        };

        document.addEventListener('touchmove', handleTouchMove);
    };

    return (<>

        {closedProfile && 
            <>
                <div className="app_profile_area closed" 
                    onAnimationEnd={(e)=>{
                        if (e.animationName === "profile_close_background")
                            onChange(false)
                    }}>

                    <div className="app_profile_area_panel closed">

                    </div>
                </div>
            </>
        }
        
        {!closedProfile &&
            <>
                <div className="app_profile_area" onTouchStart={handleTouchStart}>
                    <div className="app_profile_area_panel"  onMouseLeave={()=>setClosedProfile(true)}>

                    </div>
                </div>
            </>
        }
    </>)
}

export default ProfilePage;