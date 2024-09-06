<script setup>
    import { useRouter } from 'vue-router';
    import { useStore } from "vuex" 
    import { ref,onMounted} from 'vue'
    import { stIdSearch } from "@/api/rider"
    import { getStationsInfo } from "@/api/platform"
    import imgurl from '@/assets/my_logo.png';
    import screenfull from "screenfull";

    const router = useRouter()
    const store = useStore()    
    const rider = ref({})
    const stationId = ref(null); // 用于存储站点 ID  
    const riderStation = ref({}); // 用于存储站点信息  
    const ifStation = ref(true); // 用于判断是否有站点信息   
    
    const username  = store.state.rider.RiderId
    const message = 2

    const handleCommand = (command) => {
        if (command == 'loginout') {
            store.dispatch('clearRider'); 
            router.push('/login');
        } else if (command == 'user') {
            router.push('/rider-home/information')
        }
    }

    const setFullScreen = () => {
        if (screenfull.isEnabled && !screenfull.isFullscreen) {
            screenfull.request();
        }
        if (screenfull.isEnabled && screenfull.isFullscreen) {
            screenfull.exit();
        }
    }


    onMounted(() => {  
        // 从 cookie 中读取用户信息  
        const riderData = store.state.rider; 
        if (riderData) {  
            rider.value = riderData;  
        } else {   
            router.push('/login');
        }  
        stIdSearch(rider.value.RiderId).then(res => {  
            stationId.value = res.data; 
            getStationsInfo(stationId.value).then(res => {  
            riderStation.value = res.data; 
            }).catch(err => {  
                riderStation.value = "站点信息获取失败"; 
            });  
        }).catch(err => {  
            ifStation.value = false;  
        });   
    });  


</script>

<template>
    <div class="header">

        <!-- 顶部栏左侧，包含logo，名称以及站点信息 -->
        <div class="headerLeft">
            <img class="logo" src="@/assets/my_logo.png" alt="" />
            <div class="riderTitle">外卖骑手网站</div>
            <div v-if="ifStation" class="stationInfo">
                当前站点：{{ riderStation.stationName }}&nbsp;
                地址：{{ riderStation.stationAddress }}&nbsp;
            </div>
            <div v-else class="stationInfo">
                请等待站点分配&nbsp;
            </div>
        </div>

        <div class="headerRight">
            <div class="headerUsercon">
                <div class="btn-icon" @click="setFullScreen">
                    <el-tooltip effect="dark" content="全屏" placement="bottom">
                        <el-icon style="font-size: 24px; transform:rotate(45deg)"><Rank /></el-icon>
                    </el-tooltip>
                </div>
                <!-- 用户头像 -->
                <el-avatar class="user-avator" :size="30" :src="imgurl"  />

                <!-- 用户名下拉菜单 -->
                <el-dropdown class="user-name" trigger="click" @command="handleCommand">
                    <span class="el-dropdown-link">
                        {{ username }}
                        <el-icon><ArrowDown /></el-icon>
                    </span>
                    <template #dropdown>
                        <el-dropdown-menu>
                            <el-dropdown-item command="user">个人中心</el-dropdown-item>
                            <el-dropdown-item divided command="loginout">退出登录</el-dropdown-item>
                        </el-dropdown-menu>
                    </template>
                </el-dropdown>
            </div>
        </div>
    </div>
</template>

<style scoped>
.header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    box-sizing: border-box;
    width: 100%;
    height: 70px;
    background: #393d49;
    color:aliceblue;
    border-bottom: 1px solid #ddd;

}

.headerLeft {
    display:flex;
    align-items: center;
    padding-left: 20px;
    height: 100%;
}

.logo {
    width: 35px;
}

.riderTitle {
    margin: 0 40px 0 10px;
    font-size: 20px;
}
.stationInfo {
    margin: 0 40px 0 10px;
    font-size: 20x;
}

.headerRight {
    float: right;
    padding-right: 50px;
}

.headerUsercon {
    display: flex;
    height: 100%;
    align-items: center;
}

.btn-icon {
    position: relative;
    width: 30px;
    height: 30px;
    text-align: center;
    cursor: pointer;
    display: flex;
    align-items: center;
    margin: 0 5px;
    font-size: 20px;
}

.btn-bell-badge {
    position: absolute;
    right: 4px;
    top: 0px;
    width: 8px;
    height: 8px;
    border-radius: 4px;
    background: #f56c6c;
    color: var(--header-text-color);
}

.user-avator {
    margin: 0 10px 0 20px;
    background-color: #FFF;
}

.el-dropdown-link {
    color:aliceblue;
    cursor: pointer;
    display: flex;
    align-items: center;
}

.el-dropdown-menu__item {
    text-align: center;
}
</style>