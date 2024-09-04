<script setup>
import { useStore } from "vuex";
import { ElMessage, ElMessageBox } from "element-plus";
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { getMerchantIds, getMerchantsInfo, getAllMerchantsInfo } from "@/api/user";
import { merchantDeleteService } from "@/api/platform";
import { getMerAvgRating } from "@/api/merchant";
const router = useRouter();
const merchantIds = ref([]);  //获取所有商家id
const merchantsInfo = ref([]); // 存储所有商家信息 
const showMerchantsInfo = ref([]); // 显示商家信息列表
const searchQuery = ref(''); // 搜索框内容
const isMerchantInfo = ref(false); // 是否显示商家信息
const currentMerchantId = ref(null); // 当前商家id
const MerchantInfo = ref({});  //商户信息

const sortField = ref(''); // 搜索框内容
const sortOrder = ref('1'); // 搜索框内容
const parentBorder = ref(false);
const childBorder = ref(false);
onMounted(async () => {

    // getMerchantIds().then(res => {  // 获取所有商家id
    //     merchantIds.value = res.data;  
    //     return Promise.all(merchantIds.value.map(id => getMerchantsInfo(id))); // 并发请求所有商家信息
    // }).then(responses => {  
    //     merchantsInfo.value = responses.map(response => response.data); // 提取商家信息  
    //     showMerchantsInfo.value = merchantsInfo.value; // 显示商家信息列表
    // }).catch(err => {  
    //     ElMessage.error('获取商家id失败'); 
    // }); 
    getMerchantIds().then(res => {
        merchantIds.value = res.data;
    });
    getAllMerchantsInfo().then(res => {
        merchantsInfo.value = res.data;
        showMerchantsInfo.value = merchantsInfo.value;
        // fetchDefaultAddress();
    })
    await fetchMerAvgRating();
})

const sortCoupons = () => {
    getAllMerchantsInfo(sortField.value, sortOrder.value).then(res => {
        merchantsInfo.value = res.data;
        showMerchantsInfo.value = merchantsInfo.value;
    })
}
const fetchMerAvgRating = async () => {
    try {
        for (let info of showMerchantsInfo.value) {
            const avgRating = await getMerAvgRating(info.merchantId);
            console.log(`Merchant ID: ${info.merchantId}, Avg Rating: ${avgRating}`);
            info.avgRating = avgRating; // 将平均评分添加到商家信息对象中
        }
        console.log("商家信息", showMerchantsInfo.value);
    } catch (error) {
        console.error('Failed to fetch merchant average ratings:', error);
    }

};
const formatTime = (seconds) => {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;
}
const handleSearch = () => {   //字符串匹配搜索商家
    const query = searchQuery.value.trim();
    if (query) {
        showMerchantsInfo.value = merchantsInfo.value.filter(merchant =>
            merchant.merchantName.includes(query) ||
            merchant.dishType.includes(query)
        );
    } else {
        showMerchantsInfo.value = merchantsInfo.value;
    }
};
const enterMerchantInfo = (merchantId) => {  // 进入商家管理页面
    currentMerchantId.value = merchantId;
    MerchantInfo.value = merchantsInfo.value.find(merchant => merchant.merchantId === merchantId);
    MerchantInfo.value.timeforOpenBusiness = formatTime(MerchantInfo.value.timeforOpenBusiness);  //时间转换
    MerchantInfo.value.timeforCloseBusiness = formatTime(MerchantInfo.value.timeforCloseBusiness);
    isMerchantInfo.value = true;
};
const deleteMerchant = (merchantId) => {  // 销号
    ElMessageBox.confirm('确认要删除该商家吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
    }).then(() => {
        merchantDeleteService(merchantId).then(res => {
            ElMessage.success('销号成功');
            showMerchantsInfo.value = showMerchantsInfo.value.filter(merchant => merchant.merchantId !== merchantId);
            isMerchantInfo.value = false;
            currentMerchantId.value = null;
            MerchantInfo.value = {};
        }).catch(err => {
            ElMessage.error('销号失败');
        });
    }).catch(() => { });
};
const gobackHome = () => {
    router.push('/platform-home');
}
</script>

<template>
    <div v-if="!isMerchantInfo" class="box">
        <div class="head">商家列表</div>
        <div class="top">
            <input type="text" v-model="searchQuery" placeholder="搜索店名或类别" v-on:keyup.enter="handleSearch()" class="inputtext" />
            <button @click="handleSearch()" class="search">搜索</button>
        </div>

        <el-table :data="showMerchantsInfo" :border="parentBorder" style="width:100%" class="table">
            <el-table-column type="expand">
                <template #default="props">
                    <div m="4" style="margin-left: 10%;">
                        <p m="t-0 b-2"> 商家信息</p>
                        <p m="t-0 b-2"> 地址：{{ props.row.merchantAddress }}</p>
                        <p m="t-0 b-2"> 营业时间：{{ formatTime(props.row.timeforOpenBusiness) }}-{{
                            formatTime(props.row.timeforCloseBusiness) }}</p>
                        <p m="t-0 b-2">  联系电话：{{ props.row.contact }}</p>
                        <p m="t-0 b-2">  是否可以使用通用优惠券：{{ props.row.couponType ? '否' : '是' }}</p>
                        <p m="t-0 b-2">  商家评分：{{ props.row.avgRating }}</p>
                        <button @click="deleteMerchant(currentMerchantId)" class="choose-des">销号</button>

                    </div>
                </template>

            </el-table-column>
            <el-table-column label="商家" prop="merchantName" />
            <el-table-column label="商品类别" prop="dishType" />
        </el-table>
        <button @click="gobackHome" class="return">返回</button>
    </div>


</template>
<style scoped></style>
<style scoped lang="scss">
.choose-des{
    padding: 5px 8px;         /* 按钮内边距 */
    margin-right: 1px;         /* 按钮右边距 */
    background-color: #f190a0;
    font-size: 2vmin; /* 字体大小 */
    border-radius: 9px;
}
.choose-des:hover{
    background-color: #FFC0CB;
}
.table{
    margin-top:10px;

    height:70%;
    width:80%;
    border-radius: 10px;
    border: 2px solid #01042a;
    table-layout: auto;
    margin-top: 1%;
}
.top{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    margin-bottom: 2%;
}
.box{
    padding: 20px;
    background-color: #7ac2ee;
    border: 2px solid #000000;
    border-radius: 20px;
    margin-right: 30px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);

    font-size: 3vmin; /* 字体大小 */
    position: fixed; /* 固定定位 */
    top: 60px; /* 贴近顶部 */
    left: 50%; /* 水平居中 */
    transform: translateX(-50%); /* 修正水平居中 */
    width: 70%;
    height: 80%;

}

.return{
    position: absolute;
    right: 10px;
    bottom: 10px;
    background-color: #FFC0CB;
    font-size: 2.5vmin;
}
.return:hover{
    background-color: #f7ced5;
}
.head{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    left:50%;
    font-size: 4vmin; /* 字体大小 */
    color:#000000;
    margin-bottom: 1%;
}
.head4{
    display:flex;
    justify-content:center; 
    align-items: center; /* 垂直居中 */
    left:50%;
    font-size: 3.5vmin; /* 字体大小 */
    color:#000000;
    margin-bottom: 1%;
}
.choose{
    padding: 5px 8px;         /* 按钮内边距 */
    margin-right: 1px;         /* 按钮右边距 */
    background-color: #f190a0;
    font-size: 2.5vmin; /* 字体大小 */
    border-radius: 9px;
}
.choose:hover{
    background-color: #FFC0CB;
}
.head2{
    margin-top:4%;
    margin-bottom: 3%;
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    color:#f56a81;
}
.inputtext{
    height: 25px;
    width: 450px;
    right:5%;
    font-size: 2.2vmin;
    border-radius: 9px;
    margin-right: 0%;
}
.input{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    margin-left:30%;
    margin-right:30%;
    width:40%;
    font-size: 3vmin;
}
.output{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    margin-top:1%;
   // width:100%;
    font-size: 3vmin;
    color:#583def;
}
.search{
    padding: 6px 9px;         /* 按钮内边距 */
    margin-right: 8px;         /* 按钮右边距 */
    background-color: #f3adb8;
    font-size: 2.5vmin; /* 字体大小 */
    border-radius: 9px;
    margin-left: 10px;
}
</style>