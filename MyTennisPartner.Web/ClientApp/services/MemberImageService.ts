import AppConstants from '../models/app-constants';
import RestUtilities from './RestUtilities';
import AuthStore from '../stores/Auth';
import MemberViewModel from "../models/viewmodels/MemberViewModel";

interface IMemberImageResponse {
    result: MemberViewModel;
}

export default class MemberImageService {

    createMemberImage(memberId: number, file: File) {
        var form = new FormData();
        form.append('files', file);
        return RestUtilities.postFile<MemberViewModel>(`${AppConstants.createMemberImage}/${memberId}`, form)
            .then((response) => {
                return response;
            });
    }

//    updateMemberImage(memberId: number, images: any) {
//        let form = new FormData();
//        form.append('files[]', images);
////        form.append('memberId', memberId.toString());
//        return RestUtilities.put<MemberInfo>(`${AppConstants.updateMemberImage}/${memberId}`, form)
//            .then((response) => {
//                if (!response.is_error) {
//                    return response;
//                }
//                return response;
//            })
//            .catch((err) => {
//                alert("Caught erro on updateMemberImage in service");
//                return err;
//            });
//    }

    // file upload example
    //var input = document.querySelector('input[type="file"]')

    //var data = new FormData()
    //data.append('file', input.files[0])
    //data.append('user', 'hubot')
    
    //fetch('/avatars', {
    //    method: 'POST',
    //    body: data
    //});

    //public uploadFile(memberId: number, file: File): Promise<any> {
    //    return new Promise((resolve, reject) => {
    //        let xhr: XMLHttpRequest = new XMLHttpRequest();
    //        xhr.onreadystatechange = () => {
    //            if (xhr.readyState === 4) {
    //                if (xhr.status === 200 || xhr.status === 201) {
    //                    let res = JSON.parse(xhr.response);
    //                    resolve(res.data);
    //                } else {
    //                    let res = JSON.parse(xhr.response);
    //                    let errorObj = { Data: null, Errors: res.status == "Fail" ? res.data : res.message };
    //                    reject(errorObj);
    //                }
    //            }
    //        };

    //        let url = `${AppConstants.createMemberImage }/${memberId}`;
    //        xhr.open('POST', url, true);
    //        xhr.setRequestHeader("Authorization", `bearer ${AuthStore.getToken()}`);
    //        xhr.withCredentials = true;

    //        let formData = new FormData();
    //        formData.append('files', file);
    //        xhr.send(formData);
    //    });
    //}


}

